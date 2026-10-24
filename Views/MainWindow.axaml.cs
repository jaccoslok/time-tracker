using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TimeTracker.Helpers;
using TimeTracker.Models;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
    private bool _isCloseReminderVisible;
    private CancellationTokenSource? _feedbackCts;

    public MainWindow()
    {
        InitializeComponent();
    }

    // OnOpened fires after the window is on-screen, so setting Width/Height/Position here
    // is visible immediately and does not cause a flicker on startup.
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var s = WindowSettingsService.Load();
        Width  = s.Width;
        Height = s.Height;
        if (!double.IsNaN(s.X) && !double.IsNaN(s.Y))
            Position = new PixelPoint((int)s.X, (int)s.Y);
    }

    // OnClosing fires before the window is destroyed — last chance to write settings.
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // A paused timer has nothing unsaved; a running one would lose the current stretch
        if (ViewModel.IsTimerRunning)
        {
            e.Cancel = true;
            if (!_isCloseReminderVisible)
                _ = ShowInfoDialog("The timer is running. Pause or stop it before closing the app.");
            return;
        }

        WindowSettingsService.Save(new WindowSettings(Width, Height, Position.X, Position.Y));
        base.OnClosing(e);
    }

    // --- Projects ---

    private async void AddProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var dialogViewModel = new AddEditProjectViewModel();
        var dialog = new AddEditProjectDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            var project = new Project();
            dialogViewModel.ApplyTo(project);
            ViewModel.AddProject(project);
        }
    }

    private async void EditProjectButton_Click(object? sender, RoutedEventArgs e)
        => await EditSelectedProject();

    private async void ProjectsList_DoubleTapped(object? sender, TappedEventArgs e)
        => await EditSelectedProject();

    private async void DeleteProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var project = ViewModel.SelectedProject;
        if (project is null) return;

        if (ViewModel.IsTimerUsing(project))
        {
            await ShowInfoDialog("The timer is running for this project. Stop the timer first.");
            return;
        }

        var count = ViewModel.CountTimeEntries(project);
        var detail = count == 0
            ? "Its tasks will be deleted as well. This cannot be undone."
            : $"Its tasks and {Plural(count, "logged activity", "logged activities")} will be deleted as well. " +
              "This cannot be undone. Tip: mark the project as closed to keep its history.";

        if (await ShowConfirmDialog($"Delete project \"{project.DisplayName}\"?", detail))
            ViewModel.DeleteProject(project);
    }

    private async Task EditSelectedProject()
    {
        var project = ViewModel.SelectedProject;
        if (project is null) return;

        var dialogViewModel = new AddEditProjectViewModel(project);
        var dialog = new AddEditProjectDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            dialogViewModel.ApplyTo(project);
            ViewModel.UpdateProject(project);
        }
    }

    // --- Tasks ---

    private async void AddTaskButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProject is null) return;

        var dialogViewModel = new AddEditTaskViewModel();
        var dialog = new AddEditTaskDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            var task = new ProjectTask { ProjectId = ViewModel.SelectedProject.Id };
            dialogViewModel.ApplyTo(task);
            ViewModel.AddTask(task);
        }
    }

    private async void EditTaskButton_Click(object? sender, RoutedEventArgs e)
        => await EditSelectedTask();

    private async void TasksList_DoubleTapped(object? sender, TappedEventArgs e)
        => await EditSelectedTask();

    private async void DeleteTaskButton_Click(object? sender, RoutedEventArgs e)
    {
        var task = ViewModel.SelectedTask;
        if (task is null) return;

        if (ViewModel.IsTimerUsing(task))
        {
            await ShowInfoDialog("The timer is running for this task. Stop the timer first.");
            return;
        }

        var count = ViewModel.CountTimeEntries(task);
        var detail = count == 0
            ? "This cannot be undone."
            : $"{Plural(count, "logged activity", "logged activities")} will be deleted as well. " +
              "This cannot be undone. Tip: mark the task as closed to keep its history.";

        if (await ShowConfirmDialog($"Delete task \"{task.Name}\"?", detail))
            ViewModel.DeleteTask(task);
    }

    private async Task EditSelectedTask()
    {
        var task = ViewModel.SelectedTask;
        if (task is null) return;

        var dialogViewModel = new AddEditTaskViewModel(task);
        var dialog = new AddEditTaskDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            dialogViewModel.ApplyTo(task);
            ViewModel.UpdateTask(task);
        }
    }

    // --- Activities ---

    private async void AddActivityButton_Click(object? sender, RoutedEventArgs e)
    {
        // Preselect the task chosen in the list, otherwise the timer's task
        var defaultTaskId = ViewModel.SelectedTask?.Id ?? ViewModel.TimerTask?.Id ?? 0;

        var dialogViewModel = new ManualTimeEntryViewModel(defaultTaskId);
        var dialog = new ManualTimeEntryDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialogViewModel.TryBuildTimeEntry(out var entry))
            ViewModel.AddTimeEntry(entry!);
    }

    private async void EditActivityButton_Click(object? sender, RoutedEventArgs e)
        => await EditSelectedActivity();

    private async void ActivitiesList_DoubleTapped(object? sender, TappedEventArgs e)
        => await EditSelectedActivity();

    private async void DeleteActivityButton_Click(object? sender, RoutedEventArgs e)
    {
        var row = ViewModel.SelectedActivity;
        if (row is null) return;

        if (await ShowConfirmDialog("Delete this activity?", "This cannot be undone."))
            ViewModel.DeleteTimeEntry(row);
    }

    private async Task EditSelectedActivity()
    {
        var row = ViewModel.SelectedActivity;
        if (row is null) return;

        // Load a fresh copy without navigation properties, so changing the task is safe to save
        var entry = ViewModel.GetTimeEntry(row.Id);
        if (entry is null) return;

        var dialogViewModel = new ManualTimeEntryViewModel(entry);
        var dialog = new ManualTimeEntryDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialogViewModel.TryApplyToEntry(entry))
            ViewModel.UpdateTimeEntry(entry);
    }

    private async void CopyRemarksMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        // The menu item inherits the row's DataContext, so this pattern gets the ActivityRow
        if (sender is not MenuItem { DataContext: ActivityRow row }) return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) return;

        await clipboard.SetTextAsync(row.Remarks);
        await ShowFeedbackAsync("Copied");
    }

    // --- Export ---

    private async void ExportCsvButton_Click(object? sender, RoutedEventArgs e)
    {
        var file = await PickSaveFileAsync("CSV file", "csv");
        if (file is null) return;

        await using (var stream = await file.OpenWriteAsync())
            ActivityExporter.WriteCsv(stream, ViewModel.Activities);

        await ShowFeedbackAsync("Exported");
    }

    private async void ExportExcelButton_Click(object? sender, RoutedEventArgs e)
    {
        var file = await PickSaveFileAsync("Excel workbook", "xlsx");
        if (file is null) return;

        await using (var stream = await file.OpenWriteAsync())
            ActivityExporter.WriteExcel(stream, ViewModel.Activities);

        await ShowFeedbackAsync("Exported");
    }

    private async Task<IStorageFile?> PickSaveFileAsync(string typeName, string extension)
    {
        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null) return null;

        return await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Activities",
            SuggestedFileName = $"Time Tracker - {ViewModel.SelectedPeriodName}.{extension}",
            FileTypeChoices = [new FilePickerFileType(typeName) { Patterns = [$"*.{extension}"] }]
        });
    }

    // Shows a short message next to the export buttons for about a second
    private async Task ShowFeedbackAsync(string text)
    {
        // Cancel a previous message that is still showing, so they don't hide each other early
        _feedbackCts?.Cancel();
        _feedbackCts?.Dispose();
        _feedbackCts = new CancellationTokenSource();
        var token = _feedbackCts.Token;

        FeedbackText.Text = text;
        FeedbackText.IsVisible = true;

        try
        {
            await Task.Delay(1200, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        FeedbackText.IsVisible = false;
    }

    // --- Shared dialogs ---

    private static string Plural(int count, string singular, string plural) =>
        count == 1 ? $"1 {singular}" : $"{count} {plural}";

    private async Task<bool> ShowConfirmDialog(string message, string detail)
    {
        var dialog = new Window
        {
            Title = "Confirm",
            Width = 420,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };

        var deleteButton = new Button { Content = "Delete", Width = 80 };
        var cancelButton = new Button { Content = "Cancel", Width = 80, IsCancel = true };

        deleteButton.Click += (_, _) => dialog.Close(true);
        cancelButton.Click += (_, _) => dialog.Close(false);

        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = message, FontWeight = Avalonia.Media.FontWeight.Bold,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new TextBlock { Text = detail, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 8,
                    Margin = new Thickness(0, 6, 0, 0),
                    Children = { cancelButton, deleteButton }
                }
            }
        };

        return await dialog.ShowDialog<bool>(this);
    }

    private async Task ShowInfoDialog(string message)
    {
        _isCloseReminderVisible = true;

        var dialog = new Window
        {
            Title = "Time Tracker",
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 360,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };

        var okButton = new Button { Content = "OK", Width = 80, IsDefault = true };
        okButton.Click += (_, _) => dialog.Close();

        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 18,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap, MaxWidth = 400 },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Children = { okButton }
                }
            }
        };

        await dialog.ShowDialog(this);
        _isCloseReminderVisible = false;
    }
}
