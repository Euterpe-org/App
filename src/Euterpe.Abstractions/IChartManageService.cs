namespace Euterpe.Abstractions;

public interface IChartManageService
{
    // Stream + lifecycle
    IObservable<IChangeSet<ChartDto, string>> Connect();
    Task InitializeChartsAsync();
    IReadOnlyList<int> GetOnlineChartCids();

    // Single-chart operations
    Task DownloadChartAsync(string cid, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default);
    Task UpdateChartAsync(string cid, CancellationToken cancellationToken = default);
    Task RemoveChartAsync(string folderPath);
    Task RefreshChartAsync(string folderPath);

    // Bulk operations
    Task<int> UpdateAllChartsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BulkItemResult>> DownloadChartsAsync(IReadOnlyList<string> cids, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default);
    Task<int> DeleteChartsAsync(IReadOnlyList<string> folderPaths, CancellationToken cancellationToken = default);
    Task<bool> ImportChartsAsync(IReadOnlyList<string> paths);

    // Local folder reconciliation
    Task ReconcileChartsAsync();
}
