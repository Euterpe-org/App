using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Shell;

public sealed partial class MainWindow : UrsaWindow
{
    public WindowNotificationManager Notifier { get; }

    public MainWindow()
    {
        InitializeComponent();

        Notifier = new WindowNotificationManager(this);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (e.CloseReason is WindowCloseReason.WindowClosing
            && DataContext is MainWindowViewModel { Config.MinimizeToTrayOnClose: true })
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
