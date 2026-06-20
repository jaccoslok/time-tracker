using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TimeTracker.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
