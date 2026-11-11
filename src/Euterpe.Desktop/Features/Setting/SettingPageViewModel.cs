namespace Euterpe.Features.Setting;

[Route("/setting", DisplayName = nameof(Page_Setting), Icon = "Setting", Order = 4)]
[AppSingleton]
public sealed partial class SettingPageViewModel : ViewModelBase
{
    public static IReadOnlyList<EnumOption<UpdateChannel>> UpdateChannels { get; } =
    [
        .. UpdateChannelExtensions.GetValues().Select(static channel =>
            new EnumOption<UpdateChannel>(channel))
    ];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyLanguageCommand))]
    public partial Language? CurrentLanguage { get; set; }

    public Language[] AvailableLanguages => LocalizationService.AvailableLanguages;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        CurrentLanguage = LocalizationService.GetCurrentLanguage();

        Logger.LogInformation("{ViewModel} Initialized", nameof(SettingPageViewModel));
    }

    [RelayCommand(CanExecute = nameof(CanExecuteApplyLanguage))]
    private void ApplyLanguage() => LocalizationService.SetLanguage(CurrentLanguage!.Name);

    private bool CanExecuteApplyLanguage() => CurrentLanguage is not null;

    [RelayCommand]
    private async Task ChangeGameFolderAsync(GameConfig game) =>
        game.Folder = await GameLocalServiceFor(game.Id).GetGameFolderAsync().ConfigureAwait(false);

    [RelayCommand]
    private async Task ChangeCacheFolderAsync() =>
        Config.CacheFolder = await AppLocalService.GetCacheFolderAsync().ConfigureAwait(false);

    #region Injections

    public required Config Config { get; init; }
    public required LocalizationService LocalizationService { get; init; }
    public required IAppLocalService AppLocalService { get; init; }
    public required Func<GameId, IGameLocalService> GameLocalServiceFor { get; init; }
    public required ILogger<SettingPageViewModel> Logger { get; init; }

    #endregion Injections
}
