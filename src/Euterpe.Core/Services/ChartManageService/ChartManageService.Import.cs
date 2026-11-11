using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task<bool> ImportChartsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default)
    {
        var imported = 0;

        foreach (var path in paths)
        {
            if (await ImportChartAsync(path, cancellationToken).ConfigureAwait(false))
            {
                imported++;
            }
        }

        return imported > 0;
    }

    private async Task<bool> ImportChartAsync(string path, CancellationToken cancellationToken)
    {
        var name = Directory.Exists(path) ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        var kind = ClassifyChartDrop(path);

        if (kind is ChartDropKind.Unsupported)
        {
            Logger.LogWarning("Ignored dropped path (not a chart package or folder): {Path}", path);
            NotificationService.ErrorLight(Notification_Content_Chart_Import_Unsupported(name));
            return false;
        }

        try
        {
            var (outcome, destination) = kind is ChartDropKind.Package
                ? await ExtractPackagedChartAsync(path, name).ConfigureAwait(false)
                : await MigrationService.MigrateCustomAlbumAsync(ChartLocalService.CreateCustomAlbumSource(path), cancellationToken).ConfigureAwait(false);

            if (outcome is MigrationOutcome.Migrated)
            {
                await CacheLocalChartAsync(destination, ChartSource.Offline).ConfigureAwait(false);
            }

            NotifyImportOutcome(outcome, name, path);
            return outcome is MigrationOutcome.Migrated;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to import chart from {Path}", path);
            NotificationService.ErrorLight(Notification_Content_Chart_Import_Failed(name));
            return false;
        }
    }

    private static ChartDropKind ClassifyChartDrop(string path)
    {
        if (Directory.Exists(path))
        {
            return File.Exists(Path.Combine(path, CustomAlbumFiles.InfoFileName))
                ? ChartDropKind.CustomAlbum
                : ChartDropKind.Unsupported;
        }

        if (!File.Exists(path))
        {
            return ChartDropKind.Unsupported;
        }

        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            CustomAlbumFiles.PackageExtension => ChartDropKind.CustomAlbum,
            ChartFiles.PackageExtension => ChartDropKind.Package,
            _ => ChartDropKind.Unsupported
        };
    }

    private async Task<MigrationResult> ExtractPackagedChartAsync(string path, string name)
    {
        var desiredFolder = Path.Combine(GameConfig.OfflineChartsFolder, name);
        var workFolder = Path.Combine(GameConfig.TempChartsFolder, Guid.NewGuid().ToString("N"));

        try
        {
            await Archive.ExtractZipFileAsync(path, workFolder).ConfigureAwait(false);

            return FileSystemService.TryMoveDirectoryToAvailablePath(workFolder, desiredFolder, out var destination)
                ? new MigrationResult(MigrationOutcome.Migrated, destination)
                : new MigrationResult(MigrationOutcome.Failed, desiredFolder);
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }

    private void NotifyImportOutcome(MigrationOutcome outcome, string name, string path)
    {
        switch (outcome)
        {
            case MigrationOutcome.Migrated:
                Logger.LogInformation("Imported chart {Name} from {Path}", name, path);
                NotificationService.SuccessLight(Notification_Content_Chart_Import_Success(name));
                break;
            case MigrationOutcome.Unsupported:
                Logger.LogInformation("Import of {Name} skipped: chart cannot be migrated", name);
                NotificationService.ErrorLight(Notification_Content_Chart_Import_Unmigratable(name));
                break;
            case MigrationOutcome.Failed:
                NotificationService.ErrorLight(Notification_Content_Chart_Import_Failed(name));
                break;
            default:
                throw new UnreachableException();
        }
    }

    private enum ChartDropKind
    {
        CustomAlbum,
        Package,
        Unsupported
    }
}
