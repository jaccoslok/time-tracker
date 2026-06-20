using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TimeTracker.Views;

public partial class AddEditProjectDialog : Window
{
    public AddEditProjectDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(true); // returns true to the caller that awaited ShowDialog
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
