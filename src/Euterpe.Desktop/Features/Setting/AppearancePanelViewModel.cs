namespace Euterpe.Features.Setting;

[Route("/setting/appearance", DisplayName = Panel_Setting_Appearance, Order = 1)]
[AppSingleton]
public sealed partial class AppearancePanelViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyLanguageCommand))]
    public partial Language? CurrentLanguage { get; set; }

    public Language[] AvailableLanguages => LocalizationService.AvailableLanguages;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        CurrentLanguage = LocalizationService.GetCurrentLanguage();

        Logger.LogInformation("{ViewModel} Initialized", nameof(AppearancePanelViewModel));
    }

    [RelayCommand(CanExecute = nameof(CanExecuteApplyLanguage))]
    private void ApplyLanguage() => LocalizationService.SetLanguage(CurrentLanguage!.Name);

    private bool CanExecuteApplyLanguage() => CurrentLanguage is not null;

    #region Injections

    public required LocalizationService LocalizationService { get; init; }
    public required ILogger<AppearancePanelViewModel> Logger { get; init; }

    #endregion Injections
}
