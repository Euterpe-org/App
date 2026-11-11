using System.Collections.Concurrent;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task<int> MigrateCustomAlbumsAsync(IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(GameConfig.CustomAlbumsChartsFolder))
        {
            Logger.LogInformation("No CustomAlbums folder at {CustomAlbumsChartsFolder}, nothing to migrate", GameConfig.CustomAlbumsChartsFolder);
            return 0;
        }

        var sources = ChartLocalService.GetCustomAlbumSources();
        var processed = await MigrateSourcesAsync(sources, progress, cancellationToken).ConfigureAwait(false);

        if (!Directory.EnumerateFileSystemEntries(GameConfig.CustomAlbumsChartsFolder).Any())
        {
            FileSystemService.TryDeleteDirectory(GameConfig.CustomAlbumsChartsFolder, DeleteOption.IgnoreIfNotFound);
        }

        return processed;
    }

    public Task<int> MigrateCustomAlbumFilesAsync(
        IReadOnlyList<string> filePaths,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        MigrateSourcesAsync(filePaths.Select(static path => new CustomAlbumSource(path, false)).ToArray(), progress, cancellationToken);

    private async Task<int> MigrateSourcesAsync(
        CustomAlbumSource[] sources,
        IProgress<BatchProgress>? progress,
        CancellationToken cancellationToken)
    {
        var outcomes = new ConcurrentBag<MigrationOutcome>();
        var completed = 0;

        await Parallel.ForEachAsync(
                sources,
                new ParallelOptions { CancellationToken = cancellationToken },
                async (source, token) =>
                {
                    outcomes.Add(await MigrateSourceAsync(source, token).ConfigureAwait(false));
                    progress?.Report(new BatchProgress(Interlocked.Increment(ref completed), sources.Length));
                })
            .ConfigureAwait(false);

        var migrated = outcomes.Count(outcome => outcome is MigrationOutcome.Migrated);
        var unsupported = outcomes.Count(outcome => outcome is MigrationOutcome.Unsupported);
        var failed = outcomes.Count(outcome => outcome is MigrationOutcome.Failed);
        Logger.LogInformation("CustomAlbums migration complete: {Migrated} migrated, {Unsupported} unsupported, {Failed} failed", migrated, unsupported, failed);

        var notMigrated = failed + unsupported;
        if (notMigrated > 0)
        {
            NotificationService.WarningLight(Notification_Content_Migration_Partial(migrated, notMigrated));
            return migrated + notMigrated;
        }

        if (migrated > 0)
        {
            NotificationService.SuccessLight(Notification_Content_Migration_Success(migrated));
        }

        return migrated;
    }

    private async Task<MigrationOutcome> MigrateSourceAsync(CustomAlbumSource source, CancellationToken cancellationToken)
    {
        var (outcome, destination) = await MigrationService.MigrateCustomAlbumAsync(source, cancellationToken).ConfigureAwait(false);
        if (outcome is not MigrationOutcome.Migrated)
        {
            return outcome;
        }

        RemoveCustomAlbumSource(source);
        await CacheLocalChartAsync(destination, ChartSource.Offline).ConfigureAwait(false);
        return outcome;
    }

    private void RemoveCustomAlbumSource(CustomAlbumSource source)
    {
        if (source.IsFolder)
        {
            FileSystemService.TryDeleteDirectory(source.Path, DeleteOption.IgnoreIfNotFound);
        }
        else
        {
            FileSystemService.TryDeleteFile(source.Path);
        }
    }
}
