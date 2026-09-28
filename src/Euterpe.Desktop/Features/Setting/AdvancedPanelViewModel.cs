namespace Euterpe.Features.Setting;

[Route("/setting/advanced", DisplayName = Panel_Setting_Advanced, Order = 5)]
[AppSingleton]
public sealed class AdvancedPanelViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(AdvancedPanelViewModel));
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<AdvancedPanelViewModel> Logger { get; init; }

    #endregion Injections
}
