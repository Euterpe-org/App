namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private async Task InitializeCoreAsync()
    {
        _sourceCache.AddOrUpdate(await LoadLocalChartsAsync(GetLocalChartFolders()).ConfigureAwait(false));
        Logger.LogInformation("All charts loaded");

        StartWatching();
    }

    private async Task DownloadChartCoreAsync(string cid, IProgress<BatchProgress>? progress, CancellationToken cancellationToken)
    {
        try
        {
            var folderPath = await GameDownloadManager.DownloadChartAsync(cid, progress, cancellationToken).ConfigureAwait(false);
            if (await CacheLocalChartAsync(folderPath, ChartSource.Online).ConfigureAwait(false) is not { } chart)
            {
                NotificationService.ErrorLight(Notification_Content_Chart_Download_Failed(cid));
                return;
            }

            Logger.LogInformation("Chart {Cid} downloaded and added to cache", cid);
            NotificationService.SuccessLight(Notification_Content_Chart_Download_Success(chart.Manifest.Meta.Name));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to download chart {Cid}", cid);
            NotificationService.ErrorLight(Notification_Content_Chart_Download_Failed(cid));
        }
    }

    private async Task<BulkItemOutcome> DownloadChartForBulkAsync(string cid, CancellationToken cancellationToken)
    {
        try
        {
            var folderPath = await GameDownloadManager.DownloadChartAsync(cid, null, cancellationToken).ConfigureAwait(false);
            return await CacheLocalChartAsync(folderPath, ChartSource.Online).ConfigureAwait(false) is not null
                ? BulkItemOutcome.Added
                : BulkItemOutcome.Failed;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "Failed to download chart {Cid} during bulk import", cid);
            return BulkItemOutcome.Failed;
        }
    }

    private async Task<ChartUpdateResult> UpdateChartCoreAsync(string cid, IReadOnlyCollection<string> changedFiles, IReadOnlyCollection<string> deletedFiles,
        CancellationToken cancellationToken)
    {
        try
        {
            var folderPath = await GameDownloadManager.UpdateChartAsync(cid, changedFiles, deletedFiles, cancellationToken).ConfigureAwait(false);
            if (await CacheLocalChartAsync(folderPath, ChartSource.Online).ConfigureAwait(false) is not { } chart)
            {
                return new ChartUpdateResult(false, cid);
            }

            Logger.LogInformation("Chart {Cid} updated", cid);
            return new ChartUpdateResult(true, chart.Manifest.Meta.Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update chart {Cid}", cid);
            return new ChartUpdateResult(false, cid);
        }
    }

    private Task RemoveChartCoreAsync(string folderPath)
    {
        Logger.LogInformation("Removing chart at {FolderPath}", folderPath);

        var name = _sourceCache.Lookup(folderPath) is { HasValue: true, Value: var chart }
            ? chart.Manifest.Meta.Name
            : Path.GetFileName(folderPath);

        if (RemoveLocalChart(folderPath))
        {
            Logger.LogInformation("Chart at {FolderPath} removed", folderPath);
            NotificationService.SuccessLight(Notification_Content_Chart_Remove_Success(name));
        }
        else
        {
            Logger.LogError("Failed to remove chart at {FolderPath}", folderPath);
            NotificationService.ErrorLight(Notification_Content_Chart_Remove_Failed(name));
        }

        return Task.CompletedTask;
    }

    private async Task RefreshChartCoreAsync(string folderPath)
    {
        var fullPath = Path.GetFullPath(folderPath);
        var existing = _sourceCache.Items.FirstOrDefault(chart =>
            string.Equals(Path.GetFullPath(chart.FolderPath), fullPath, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            return;
        }

        await CacheLocalChartAsync(existing.FolderPath, existing.Source).ConfigureAwait(false);
    }

    private Task RemoveDelistedChartCoreAsync(string cid)
    {
        if (FindOnlineChartByCid(cid) is not { } chart)
        {
            return Task.CompletedTask;
        }

        if (RemoveLocalChart(chart.FolderPath))
        {
            Logger.LogInformation("Removed delisted chart {Cid}", cid);
        }
        else
        {
            Logger.LogError("Failed to remove delisted chart {Cid}", cid);
        }

        return Task.CompletedTask;
    }

    private async Task<ChartDto?> CacheLocalChartAsync(string chartFolder, ChartSource source)
    {
        var chart = await ChartLocalService.LoadChartFromPathAsync(chartFolder, source).ConfigureAwait(false);
        if (chart is null)
        {
            Logger.LogWarning("Chart at {ChartFolder} could not be loaded into the cache", chartFolder);
            return null;
        }

        _sourceCache.AddOrUpdate(chart);
        return chart;
    }

    private bool RemoveLocalChart(string folderPath)
    {
        if (!FileSystemService.TryDeleteDirectory(folderPath, DeleteOption.IgnoreIfNotFound))
        {
            return false;
        }

        _sourceCache.RemoveKey(folderPath);
        return true;
    }

    private Task RunExclusiveAsync(string key, Func<Task> action) => _singleFlight.RunAsync(key, action);

    private async Task<T> RunExclusiveAsync<T>(string key, Func<Task<T>> action)
    {
        T result = default!;
        await _singleFlight.RunAsync(key, async () => result = await action().ConfigureAwait(false)).ConfigureAwait(false);
        return result;
    }

    private readonly record struct ChartUpdateResult(bool Success, string DisplayName);
}
