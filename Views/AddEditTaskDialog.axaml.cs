using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TimeTracker.Views;

public partial class AddEditTaskDialog : Window
{
    public AddEditTaskDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
