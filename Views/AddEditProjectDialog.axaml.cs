using Avalonia.Controls;
using Avalonia.Interactivity;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class AddEditProjectDialog : Window
{
    public AddEditProjectDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var error = ((AddEditProjectViewModel)DataContext!).Validate();
        if (error is not null)
        {
            ErrorText.Text = error;
            return; // keep the dialog open so the user can correct the input
        }

        Close(true); // returns true to the caller that awaited ShowDialog
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
