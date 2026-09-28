namespace Euterpe.Features.Setting;

[Route("/setting", DisplayName = Page_Setting, Icon = "Setting", Order = 4)]
[AppSingleton]
public sealed partial class SettingPageViewModel : NavViewModelBase
{
    #region Injections

    public required ILogger<SettingPageViewModel> Logger { get; init; }

    #endregion Injections

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(SettingPageViewModel));
    }
}
