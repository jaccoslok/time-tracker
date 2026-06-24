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
        // IClassicDesktopStyleApplicationLifetime is only present in a real desktop run.
        // The Avalonia XAML previewer uses a different lifetime (no SQLite native libs),
        // so gating on this type reliably prevents the missing e_sqlite3 crash in preview.
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Apply any pending EF Core migrations at startup.
            // On a first run this creates the database schema from scratch.
            // On subsequent runs it is a no-op if the schema is already up to date.
            using (var db = new AppDbContext())
                db.Database.Migrate();

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
