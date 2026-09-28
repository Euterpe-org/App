using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Euterpe.Controls;

namespace Euterpe;

internal sealed class AppInitializer
{
    public void Run(Application app)
    {
        AsyncImage.DefaultRemoteLoader = RemoteImageLoader;

        // Initialize Window with async initializers when they are loaded
        Control.LoadedEvent.AddClassHandler<Window>(OnControlLoaded);
        AppViewModel.InitializeAsync().SafeFireAndForget();

        app.RequestedThemeVariant = AvaloniaResources.ThemeVariants[Config.Theme];
        LocalizationService.SetLanguage(Config.LanguageCode);

        SystemActivationService.SetupAsync().SafeFireAndForget(ex => Logger.LogError(ex, "Failed to register OS associations"));

        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainSplashWindow
            {
                DataContext = MainSplashWindowViewModel,
                MainWindowFactory = () =>
                {
                    var window = new MainWindow { DataContext = MainWindowViewModel };
                    NotificationServiceWiring.Notifier = window.Notifier;
                    return window;
                }
            };
            SystemActivationService.HandleStartupArgs(desktop.Args!);
        }

        if (app.ApplicationLifetime is IControlledApplicationLifetime controlled)
        {
            controlled.Exit += OnExit;
        }

        app.ObservePropertyChanged(x => x.ActualThemeVariant)
            .Subscribe(Config, static (theme, config) => config.Theme = AvaloniaResources.ThemeVariants[theme]);
    }

    private void OnControlLoaded(ContentControl control, RoutedEventArgs _)
    {
        if (control.DataContext is not IAsyncInitializable initializable)
        {
            return;
        }

        initializable.InitializeAsync().SafeFireAndForget(ex => Logger.LogError(ex, "Async initializer for {TypeName} failed", initializable.GetType().Name));
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        try
        {
            AppSettingService.Save();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save settings on exit");
        }
    }

    #region Injections

    public required Config Config { get; init; }
    public required AppViewModel AppViewModel { get; init; }
    public required MainSplashWindowViewModel MainSplashWindowViewModel { get; init; }
    public required MainWindowViewModel MainWindowViewModel { get; init; }
    public required SystemActivationService SystemActivationService { get; init; }
    public required LocalizationService LocalizationService { get; init; }
    public required IAppSettingService AppSettingService { get; init; }
    public required IRemoteImageLoader RemoteImageLoader { get; init; }
    public required ILogger<App> Logger { get; init; }
    public required INotificationServiceWiring NotificationServiceWiring { get; init; }

    #endregion
}
