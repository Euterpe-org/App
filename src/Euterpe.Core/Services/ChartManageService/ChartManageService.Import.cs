namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task<bool> ImportChartsAsync(IReadOnlyList<string> paths)
    {
        var imported = 0;

        foreach (var path in paths)
        {
            if (await ImportChartAsync(path).ConfigureAwait(false))
            {
                imported++;
            }
        }

        return imported > 0;
    }

    private async Task<bool> ImportChartAsync(string path)
    {
        var name = Directory.Exists(path) ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);

        if (!IsChartPackage(path))
        {
            Logger.LogWarning("Ignored dropped path (not a chart package): {Path}", path);
            NotificationService.ErrorLight(Notification_Content_Chart_Import_Unsupported(name));
            return false;
        }

        try
        {
            if (await ExtractPackagedChartAsync(path, name).ConfigureAwait(false) is not { } destination)
            {
                NotificationService.ErrorLight(Notification_Content_Chart_Import_Failed(name));
                return false;
            }

            await CacheLocalChartAsync(destination, ChartSource.Offline).ConfigureAwait(false);
            Logger.LogInformation("Imported chart {Name} from {Path}", name, path);
            NotificationService.SuccessLight(Notification_Content_Chart_Import_Success(name));
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to import chart from {Path}", path);
            NotificationService.ErrorLight(Notification_Content_Chart_Import_Failed(name));
            return false;
        }
    }

    private static bool IsChartPackage(string path) =>
        File.Exists(path) && Path.GetExtension(path).Equals(ChartFiles.PackageExtension, StringComparison.OrdinalIgnoreCase);

    private async Task<string?> ExtractPackagedChartAsync(string path, string name)
    {
        var desiredFolder = Path.Combine(GameConfig.OfflineChartsFolder, name);
        var workFolder = Path.Combine(GameConfig.TempChartsFolder, Guid.NewGuid().ToString("N"));

        try
        {
            await Archive.ExtractZipFileAsync(path, workFolder).ConfigureAwait(false);

            return FileSystemService.TryMoveDirectoryToAvailablePath(workFolder, desiredFolder, out var destination)
                ? destination
                : null;
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }
}
