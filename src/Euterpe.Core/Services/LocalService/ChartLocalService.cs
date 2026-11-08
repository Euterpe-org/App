using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Core;

internal sealed class ChartLocalService : IChartLocalService
{
    public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
        Directory.EnumerateDirectories(
            source switch
            {
                ChartSource.Online => GameConfig.OnlineChartsFolder,
                ChartSource.Offline => GameConfig.OfflineChartsFolder,
                _ => throw new UnreachableException()
            });

    public async Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source)
    {
        var manifestPath = Path.Combine(chartFolder, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        try
        {
            var manifest = await MessagePackSerialization.DeserializeManifestFromFileAsync(manifestPath).ConfigureAwait(false);
            if (source is ChartSource.Online && manifest.Cid is null)
            {
                Logger.LogWarning("Online chart at {ChartFolder} has no CID, skipping", chartFolder);
                return null;
            }

            return new ChartDto
            {
                FolderPath = chartFolder,
                FolderName = Path.GetFileName(chartFolder),
                Manifest = manifest,
                Source = source
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load chart from {ChartFolder}, skipping", chartFolder);
            return null;
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required ILogger<ChartLocalService> Logger { get; init; }

    #endregion Injections
}
