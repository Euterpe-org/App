namespace Euterpe.Shell;

public sealed partial class MainSplashWindow : SplashWindow
{
    public required Func<MainWindow> MainWindowFactory { get; init; }

    public MainSplashWindow()
    {
        InitializeComponent();
    }

    protected override async Task<bool> CanClose()
    {
        if (DataContext is MainSplashWindowViewModel vm)
        {
            await vm.Ready.WaitAsync().ConfigureAwait(true);
        }

        return true;
    }

    protected override Task<Window?> CreateNextWindow() => Task.FromResult<Window?>(MainWindowFactory());
}
