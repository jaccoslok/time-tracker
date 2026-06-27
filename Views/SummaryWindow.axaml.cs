using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ClosedXML.Excel;
using TimeTracker.Repositories;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class SummaryWindow : Window
{
    private CancellationTokenSource? _copiedLabelCts;
    private readonly TimeEntryRepository _timeEntryRepository = new();

    public SummaryWindow()
    {
        InitializeComponent();
        DataContext = new SummaryViewModel();
    }

    private void TodayButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SummaryViewModel viewModel)
            viewModel.SetTodayRange();
    }

    private async void ExportExcelButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SummaryViewModel viewModel) return;

        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null) return;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Summary to Excel",
            SuggestedFileName = $"timetracker-summary-{DateTime.Now:yyyyMMdd-HHmm}.xlsx",
            FileTypeChoices =
            [
                new FilePickerFileType("Excel Workbook") { Patterns = ["*.xlsx"] }
            ]
        });

        if (file is null) return;

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Summary");

        sheet.Cell(1, 1).Value = "Date";
        sheet.Cell(1, 2).Value = "Project";
        sheet.Cell(1, 3).Value = "Task";
        sheet.Cell(1, 4).Value = "Description";
        sheet.Cell(1, 5).Value = "Start";
        sheet.Cell(1, 6).Value = "Stop";
        sheet.Cell(1, 7).Value = "Duration";
        sheet.Cell(1, 8).Value = "AI Time";
        sheet.Range(1, 1, 1, 8).Style.Font.Bold = true;

        var rowIndex = 2;
        foreach (var row in viewModel.Rows)
        {
            sheet.Cell(rowIndex, 1).Value = row.Date;
            sheet.Cell(rowIndex, 2).Value = row.ProjectName;
            sheet.Cell(rowIndex, 3).Value = row.TaskName;
            sheet.Cell(rowIndex, 4).Value = row.Description;
            sheet.Cell(rowIndex, 5).Value = row.StartTime;
            sheet.Cell(rowIndex, 6).Value = row.StopTime;
            sheet.Cell(rowIndex, 7).Value = row.Duration;
            sheet.Cell(rowIndex, 8).Value = row.AiTimeSavedHours;
            rowIndex++;
        }

        rowIndex++;
        sheet.Cell(rowIndex, 1).Value = "Total hours";
        sheet.Cell(rowIndex, 2).Value = viewModel.TotalHours;
        sheet.Cell(rowIndex + 1, 1).Value = "Timed saved by AI";
        sheet.Cell(rowIndex + 1, 2).Value = viewModel.TotalAiTimeSaved;
        sheet.Range(rowIndex, 1, rowIndex + 1, 1).Style.Font.Bold = true;
        sheet.Cell(rowIndex, 2).Style.NumberFormat.Format = "0.0";
        sheet.Cell(rowIndex + 1, 2).Style.NumberFormat.Format = "0.0";

        sheet.Columns().AdjustToContents();

        await using var stream = await file.OpenWriteAsync();
        workbook.SaveAs(stream);

        await ShowCopiedLabelAsync("Exported");
    }

    private async void CopyDescriptionMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: SummaryRow row })
            await CopyDescriptionAsync(row.Description);
    }

    private async void DescriptionText_DoubleTapped(object? sender, TappedEventArgs e)
    {
        // Mark handled so it doesn't bubble up to the row's DoubleTapped handler
        e.Handled = true;
        if (sender is TextBlock { DataContext: SummaryRow row })
            await CopyDescriptionAsync(row.Description);
    }

    private async void RowGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Grid { DataContext: SummaryRow row }) return;

        var entry = _timeEntryRepository.GetById(row.Id);
        if (entry is null) return;

        var dialogViewModel = new ManualTimeEntryViewModel(entry);
        var dialog = new ManualTimeEntryDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialogViewModel.TryApplyToEntry(entry))
        {
            _timeEntryRepository.Update(entry);
            (DataContext as SummaryViewModel)?.Reload();
        }
    }

    private async void EditEntryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SummaryRow row }) return;

        var entry = _timeEntryRepository.GetById(row.Id);
        if (entry is null) return;

        var dialogViewModel = new ManualTimeEntryViewModel(entry);
        var dialog = new ManualTimeEntryDialog { DataContext = dialogViewModel };
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialogViewModel.TryApplyToEntry(entry))
        {
            _timeEntryRepository.Update(entry);
            (DataContext as SummaryViewModel)?.Reload();
        }
    }

    private async void DeleteEntryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SummaryRow row }) return;

        if (!await ShowConfirmDialog("Delete this time entry?")) return;

        _timeEntryRepository.Delete(row.Id);
        (DataContext as SummaryViewModel)?.Reload();
    }

    private async Task<bool> ShowConfirmDialog(string message)
    {
        var dialog = new Window
        {
            Title                 = "Confirm",
            Width                 = 340,
            Height                = 140,
            CanResize             = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar         = false
        };

        var yesButton = new Button { Content = "Yes", Width = 80 };
        var noButton  = new Button { Content = "No",  Width = 80 };

        yesButton.Click += (_, _) => dialog.Close(true);
        noButton.Click  += (_, _) => dialog.Close(false);

        dialog.Content = new StackPanel
        {
            Margin          = new Avalonia.Thickness(20),
            Spacing         = 16,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing     = 12,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Children    = { yesButton, noButton }
                }
            }
        };

        return await dialog.ShowDialog<bool?>(this) == true;
    }

    private async Task CopyDescriptionAsync(string? description)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) return; 

        await clipboard.SetTextAsync(description ?? string.Empty);
        await ShowCopiedLabelAsync("Copied");
    }

    private async Task ShowCopiedLabelAsync(string text)
    {
        _copiedLabelCts?.Cancel();
        _copiedLabelCts?.Dispose();
        _copiedLabelCts = new CancellationTokenSource();
        var token = _copiedLabelCts.Token;

        CopiedLabel.Text = text;
        CopiedLabel.IsVisible = true;

        try
        {
            await Task.Delay(900, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        CopiedLabel.IsVisible = false;
    }
}
