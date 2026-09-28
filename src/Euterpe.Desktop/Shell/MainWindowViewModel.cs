namespace Euterpe.Shell;

[Route("/")]
[AppSingleton]
public sealed partial class MainWindowViewModel : RootNavViewModelBase
{
    public const string DialogHostId = "MainDialogHost";

    [ObservableProperty]
    public partial GameConfig SelectedGame { get; set; } = null!;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        SelectedGame = Config.ActiveGameConfig;
        await AppSettingService.ValidateSteamAsync().ConfigureAwait(true);

        NavigationService.Ready.Set();

        Logger.LogInformation("{ViewModel} Initialized", nameof(MainWindowViewModel));
    }

    partial void OnSelectedGameChanged(GameConfig value) =>
        GameSwitcher.SwitchAsync(value.Id).SafeFireAndForget(ex => Logger.LogError(ex, "Failed to switch game to {GameId}", value.Id));

    #region Injections

    public required AuthState AuthState { get; init; }
    public required Config Config { get; init; }
    public required GameSwitcher GameSwitcher { get; init; }
    public required IAppSettingService AppSettingService { get; init; }
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    #endregion Injections
}
