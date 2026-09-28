using Velopack;
using Velopack.Sources;

namespace Euterpe.Services;

internal sealed partial class UpdateService : IUpdateService
{
    private UpdateManager? _manager;
    private UpdateInfo? _updateInfo;

    public async Task<string?> CheckForUpdatesAsync()
    {
        var runtimeIdentifier = PlatformInfo.RuntimeIdentifier;
        var channel = GetVelopackChannel(runtimeIdentifier);

        var manager = CreateUpdateManager(runtimeIdentifier, channel);
        if (!manager.IsInstalled)
        {
            Logger.LogInformation("Skipping update check because the application is not running from a Velopack installation");
            return null;
        }

        Logger.LogInformation("Checking for updates on channel {Channel}", channel);

        var updateInfo = await manager
            .CheckForUpdatesAsync()
            .WaitAsync(TimeSpan.FromSeconds(15))
            .ConfigureAwait(false);

        if (updateInfo is null)
        {
            Logger.LogInformation("No new version available");
            return null;
        }

        _manager = manager;
        _updateInfo = updateInfo;

        var newVersion = updateInfo.TargetFullRelease.Version.ToString();
        Logger.LogInformation("New version available: {NewVersion}", newVersion);
        return newVersion;
    }

    public async Task UpdateAsync(IProgress<int> progress)
    {
        progress.Report(0);

        await _manager!
            .DownloadUpdatesAsync(_updateInfo!, progress.Report)
            .ConfigureAwait(false);

        _manager.ApplyUpdatesAndRestart(_updateInfo!.TargetFullRelease);
    }

    #region Injections

    public required Config Config { get; init; }
    public required IFileDownloader FeedDownloader { get; init; }
    public required ILogger<UpdateService> Logger { get; init; }
    public required IPlatformInfo PlatformInfo { get; init; }

    #endregion Injections
}
