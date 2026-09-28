using Avalonia.Controls.ApplicationLifetimes;

namespace Euterpe.Utils;

public static class DesktopUtils
{
    public static Application GetCurrentApplication()
    {
        var app = Application.Current;
        return app ?? throw new InvalidOperationException("Application is null.");
    }

    public static IClassicDesktopStyleApplicationLifetime GetCurrentDesktop() =>
        GetCurrentApplication().ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
        ?? throw new InvalidOperationException("Desktop is null.");

    public static Window GetCurrentMainWindow()
    {
        var mainWindow = GetCurrentDesktop().MainWindow;
        return mainWindow ?? throw new InvalidOperationException("MainWindow is null.");
    }

    public static void ActivateMainWindow(bool force = false)
    {
        var mainWindow = GetCurrentMainWindow();

        if (mainWindow.WindowState is WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        if (force)
        {
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
        }

        // Defer the final activation until the current native callback has returned.
        Dispatcher.UIThread.Post(mainWindow.Activate);
    }
}
