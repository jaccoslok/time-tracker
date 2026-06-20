using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TimeTracker.Helpers;
using TimeTracker.Models;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

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
        WindowSettingsService.Save(new WindowSettings(Width, Height, Position.X, Position.Y));
        base.OnClosing(e);
    }

    // --- Projects ---

    private async void AddProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var dialogViewModel = new AddEditProjectViewModel("Add Project");
        var dialog = new AddEditProjectDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            var project = new Project
            {
                Name        = dialogViewModel.Name,
                Description = dialogViewModel.Description
            };
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

        if (await ShowConfirmDialog($"Delete project '{project.Name}'?"))
            ViewModel.DeleteProject(project);
    }

    private async Task EditSelectedProject()
    {
        var project = ViewModel.SelectedProject;
        if (project is null) return;

        var dialogViewModel = new AddEditProjectViewModel("Edit Project", project.Name, project.Description);
        var dialog = new AddEditProjectDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            project.Name        = dialogViewModel.Name;
            project.Description = dialogViewModel.Description;
            ViewModel.UpdateProject(project);
        }
    }

    // --- Tasks ---

    private async void AddTaskButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProject is null) return;

        var dialogViewModel = new AddEditTaskViewModel("Add Task");
        var dialog = new AddEditTaskDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            var task = new ProjectTask
            {
                ProjectId   = ViewModel.SelectedProject.Id,
                Name        = dialogViewModel.Name,
                Description = dialogViewModel.Description
            };
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

        if (await ShowConfirmDialog($"Delete task '{task.Name}'?"))
            ViewModel.DeleteTask(task);
    }

    private async Task EditSelectedTask()
    {
        var task = ViewModel.SelectedTask;
        if (task is null) return;

        var dialogViewModel = new AddEditTaskViewModel("Edit Task", task.Name, task.Description);
        var dialog = new AddEditTaskDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed)
        {
            task.Name        = dialogViewModel.Name;
            task.Description = dialogViewModel.Description;
            ViewModel.UpdateTask(task);
        }
    }

    // --- Time entries ---

    private async void ManualEntryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedTask is null) return;

        var dialogViewModel = new ManualTimeEntryViewModel();
        var dialog = new ManualTimeEntryDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialogViewModel.TryBuildTimeEntry(ViewModel.SelectedTask.Id, out var entry))
            ViewModel.AddTimeEntry(entry!);
    }

    private async void EditTimeEntryButton_Click(object? sender, RoutedEventArgs e)
        => await EditSelectedTimeEntry();

    private async void TimeEntriesList_DoubleTapped(object? sender, TappedEventArgs e)
        => await EditSelectedTimeEntry();

    private async void DeleteTimeEntryButton_Click(object? sender, RoutedEventArgs e)
    {
        var entry = ViewModel.SelectedTimeEntry;
        if (entry is null) return;

        if (await ShowConfirmDialog("Delete this time entry?"))
            ViewModel.DeleteTimeEntry(entry);
    }

    private async Task EditSelectedTimeEntry()
    {
        var entry = ViewModel.SelectedTimeEntry;
        if (entry is null) return;

        var dialogViewModel = new ManualTimeEntryViewModel(entry);
        var dialog = new ManualTimeEntryDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialogViewModel.TryApplyToEntry(entry))
            ViewModel.UpdateTimeEntry(entry);
    }

    // --- Summary / About ---

    private void SummaryButton_Click(object? sender, RoutedEventArgs e)
        => new SummaryWindow().Show();

    private void AboutButton_Click(object? sender, RoutedEventArgs e)
        => new AboutWindow().ShowDialog(this);

    // --- Shared confirm dialog ---

    private async Task<bool> ShowConfirmDialog(string message)
    {
        var dialog = new Window
        {
            Title  = "Confirm",
            Width  = 340,
            Height = 140,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };

        var yesButton = new Button { Content = "Yes", Width = 80 };
        var noButton  = new Button { Content = "No",  Width = 80 };

        yesButton.Click += (_, _) => dialog.Close(true);
        noButton.Click  += (_, _) => dialog.Close(false);

        dialog.Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 16,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { yesButton, noButton }
                }
            }
        };

        return await dialog.ShowDialog<bool>(this);
    }
}
