using Avalonia.Controls;
using TimeTracker.ViewModels;

namespace TimeTracker.Views;

public partial class SummaryWindow : Window
{
    public SummaryWindow()
    {
        InitializeComponent();
        DataContext = new SummaryViewModel();
    }
}
