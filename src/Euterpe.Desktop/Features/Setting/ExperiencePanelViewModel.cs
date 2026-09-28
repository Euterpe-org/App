namespace Euterpe.Features.Setting;

[Route("/setting/experience", DisplayName = Panel_Setting_Experience, Order = 2)]
[AppSingleton]
public sealed class ExperiencePanelViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(ExperiencePanelViewModel));
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<ExperiencePanelViewModel> Logger { get; init; }

    #endregion Injections
}
