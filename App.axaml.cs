using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.ViewModels;
using TimeTracker.Views;

namespace TimeTracker;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        WireNativeMenuEvents();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Apply any pending EF Core migrations at startup.
        // On a first run this creates the database schema from scratch.
        // On subsequent runs it is a no-op if the schema is already up to date.
        using (var db = new AppDbContext())
            db.Database.Migrate();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // After AXAML is loaded the NativeMenu objects exist — attach click handlers now
    private void WireNativeMenuEvents()
    {
        if (NativeMenu.GetMenu(this) is not { } menu) return;

        foreach (var item in menu.Items.OfType<NativeMenuItem>())
        {
            item.Command = item.Header switch
            {
                "About TimeTracker" => new RelayCommand(ShowAbout),
                "Quit TimeTracker"  => new RelayCommand(QuitApp),
                _                   => item.Command
            };
        }
    }

    private void ShowAbout()
    {
        var mainWindow = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow is null) return;
        new AboutWindow().ShowDialog(mainWindow);
    }

    private void QuitApp()
    {
        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }
}
