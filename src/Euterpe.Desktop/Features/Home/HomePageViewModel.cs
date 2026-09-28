using Euterpe.Core.Http.Clients;

namespace Euterpe.Features.Home;

[Route("/home", DisplayName = Page_Home, Icon = "Home", Order = 0)]
public sealed partial class HomePageViewModel : ViewModelBase
{
    public static IReadOnlyList<EnumOption<GameMode>> GameModes { get; } =
    [
        .. GameModeExtensions.GetValues().Select(static mode =>
            new EnumOption<GameMode>(mode, $"{nameof(GameMode)}_{mode.ToStringFast()}"))
    ];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        await NavigationService.Ready.WaitAsync().ConfigureAwait(true);

        await EnsureSetupAsync().ConfigureAwait(true);
        await LoadGameStateAsync().ConfigureAwait(true);
        StartBackgroundTasks();

        Logger.LogInformation("{ViewModel} Initialized", nameof(HomePageViewModel));
    }

    [RelayCommand]
    private Task LaunchGameAsync()
    {
        return GameConfig.GameMode switch
        {
            GameMode.Modded => GameLaunchService.LaunchModdedGameAsync(),
            GameMode.Vanilla => GameLaunchService.LaunchVanillaGameAsync(),
            _ => throw new UnreachableException()
        };
    }

    private async Task EnsureSetupAsync()
    {
        if (!GameConfig.SetupCompleted)
        {
            Logger.LogInformation("Setup not completed, opening full setup wizard");
            await SetupDialogService.ShowFullWizardAsync().ConfigureAwait(true);
        }
        else if (!GameSettingService.IsValidGameFolder())
        {
            Logger.LogWarning("Stored {GameName} folder is invalid, opening game path repair", GameConfig.DisplayName);
            await SetupDialogService.ShowGamePathRepairAsync().ConfigureAwait(true);
        }
    }

    private async Task LoadGameStateAsync()
    {
        GameSettingService.EnsureGameFolders();

        try
        {
            GameLocalService.ReadGameInformation();
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Read game information failed; aborting startup");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ReadGameInformation_Failed, GameConfig.GlobalGameManagersPath).ConfigureAwait(true);
            Environment.Exit(1);
            return;
        }

        GameLocalService.ReadMelonLoaderVersion();
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required NavigationService NavigationService { get; init; }
    public required SetupDialogService SetupDialogService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IDependencyAcquireService DependencyAcquireService { get; init; }
    public required IEuterpeAccountClient AccountClient { get; init; }
    public required IGameLaunchService GameLaunchService { get; init; }
    public required IGameLocalService GameLocalService { get; init; }
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }
    public required IGameSettingService GameSettingService { get; init; }
    public required ILogger<HomePageViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
