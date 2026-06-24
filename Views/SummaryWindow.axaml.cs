using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class SummaryWindow : Window
{
    private CancellationTokenSource? _copiedLabelCts;

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

    private async void CopyDescriptionMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: SummaryRow row })
            await CopyDescriptionAsync(row.Description);
    }

    private async void DescriptionText_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock { DataContext: SummaryRow row })
            await CopyDescriptionAsync(row.Description);
    }

    private async Task CopyDescriptionAsync(string? description)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) return;

        await clipboard.SetTextAsync(description ?? string.Empty);
        await ShowCopiedLabelAsync();
    }

    private async Task ShowCopiedLabelAsync()
    {
        _copiedLabelCts?.Cancel();
        _copiedLabelCts?.Dispose();
        _copiedLabelCts = new CancellationTokenSource();
        var token = _copiedLabelCts.Token;

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
