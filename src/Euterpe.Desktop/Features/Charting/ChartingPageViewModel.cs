namespace Euterpe.Features.Charting;

[Route("/charting", DisplayName = Page_Charting, Icon = "Music", Order = 2)]
public sealed partial class ChartingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new DropDownButtonItem(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_OnlineCharts, OpenFolderCommand, GameConfig.OnlineChartsFolder),
            new DropDownMenuItem(Folder_OfflineCharts, OpenFolderCommand, GameConfig.OfflineChartsFolder)
        ])
    ];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(ChartingPageViewModel));
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<ChartingPageViewModel> Logger { get; init; }

    #endregion Injections
}
