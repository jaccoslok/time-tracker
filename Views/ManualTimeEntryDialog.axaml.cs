using Avalonia.Controls;
using Avalonia.Interactivity;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class ManualTimeEntryDialog : Window
{
    public ManualTimeEntryDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = (ManualTimeEntryViewModel)DataContext!;
        var error = vm.Validate();

        if (error is not null)
        {
            ErrorText.Text = error;
            return;
        }

        ErrorText.Text = string.Empty;

        Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
}
