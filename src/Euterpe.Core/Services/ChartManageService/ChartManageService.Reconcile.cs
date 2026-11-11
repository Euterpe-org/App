namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task ReconcileChartsAsync()
    {
        var localFolders = GetLocalChartFolders();
        var addedCharts = await LoadLocalChartsAsync(FindAddedChartFolders(localFolders)).ConfigureAwait(false);
        var removedCharts = FindRemovedCharts(localFolders);

        _sourceCache.AddOrUpdate(addedCharts);
        _sourceCache.Remove(removedCharts);
        NotifyChartSync(addedCharts, removedCharts);
    }

    private async Task<ChartDto[]> LoadLocalChartsAsync(IEnumerable<string> chartFolders) =>
    [
        .. (await chartFolders
            .WhenAllAsync(folder => ChartLocalService.LoadChartFromPathAsync(folder, GetChartSource(folder))).ConfigureAwait(false))
        .OfType<ChartDto>()
    ];

    private string[] GetLocalChartFolders() =>
        [.. ChartLocalService.GetChartFolderPaths(ChartSource.Offline), .. ChartLocalService.GetChartFolderPaths(ChartSource.Online)];

    private ChartSource GetChartSource(string chartFolder) =>
        chartFolder.StartsWith(GameConfig.OnlineChartsFolder, StringComparison.OrdinalIgnoreCase)
            ? ChartSource.Online
            : ChartSource.Offline;

    private string[] FindAddedChartFolders(IEnumerable<string> chartFolders) =>
        [.. chartFolders.Where(folder => !_sourceCache.Lookup(folder).HasValue)];

    // The folder snapshot races ops caching freshly written charts, so a folder that exists on disk is never evicted.
    private ChartDto[] FindRemovedCharts(string[] localFolders)
    {
        var localKeys = localFolders.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return [.. _sourceCache.Items.Where(chart => !localKeys.Contains(chart.FolderPath) && !Directory.Exists(chart.FolderPath))];
    }

    private void NotifyChartSync(ChartDto[] addedCharts, ChartDto[] removedCharts)
    {
        switch (addedCharts, removedCharts)
        {
            case ([], []):
                break;
            case ([var addedChart], []):
                NotificationService.SuccessLight(Notification_Content_Chart_Sync_Added(addedChart.Manifest.Meta.Name));
                break;
            case ([], [var removedChart]):
                NotificationService.NoticeLight(Notification_Content_Chart_Sync_Removed(removedChart.Manifest.Meta.Name));
                break;
            default:
                NotificationService.NoticeLight(Notification_Content_Chart_Sync_Summary(addedCharts.Length, removedCharts.Length));
                break;
        }
    }
}
