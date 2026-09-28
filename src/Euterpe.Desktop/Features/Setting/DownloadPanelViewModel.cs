namespace Euterpe.Features.Setting;

[Route("/setting/download", DisplayName = Panel_Setting_Download, Order = 4)]
[AppSingleton]
public sealed class DownloadPanelViewModel : ViewModelBase
{
    public static IReadOnlyList<EnumOption<UpdateChannel>> UpdateChannels { get; } =
    [
        .. UpdateChannelExtensions.GetValues().Select(static channel =>
            new EnumOption<UpdateChannel>(channel, $"{nameof(UpdateChannel)}_{channel.ToStringFast()}"))
    ];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(DownloadPanelViewModel));
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<DownloadPanelViewModel> Logger { get; init; }

    #endregion Injections
}
