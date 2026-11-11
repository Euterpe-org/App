using System.Collections.Concurrent;
using Euterpe.Shared.Threading;

namespace Euterpe.Core;

internal sealed partial class ModManageService : IModManageService, IDisposable
{
    private readonly Lazy<Task> _initTask;
    private readonly AsyncExclusiveLock _reconcileGate = new();
    private readonly SingleFlight<string> _singleFlight = new();
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ConcurrentDictionary<string, LibDto> _libsDict = [];

    public ModManageService() => _initTask = new Lazy<Task>(InitializeCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ModDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeModsAsync() => _initTask.Value;

    public ModDto? FindModByName(string name) =>
        _sourceCache.Lookup(name) is { HasValue: true, Value: var mod } ? mod : null;

    public IReadOnlyList<ModDto> GetInstalledMods() =>
        _sourceCache.Items.Where(static mod => mod.IsLocal).ToArray();

    public Task InstallModAsync(ModDto mod) => RunAndRefreshAsync(mod, () => InstallModCoreAsync(mod));

    public async Task UpdateModAsync(ModDto mod)
    {
        var success = await RunExclusiveAsync(mod, () => UpdateModCoreAsync(mod)).ConfigureAwait(false);
        await RefreshModStatesAsync().ConfigureAwait(false);
        if (success)
        {
            NotificationService.SuccessLight(Notification_Content_Mod_Update_Success(mod.Name));
        }
        else
        {
            NotificationService.ErrorLight(Notification_Content_Mod_Update_Failed(mod.Name));
        }
    }

    public Task ReinstallModAsync(ModDto mod) => RunAndRefreshAsync(mod, () => ReinstallModCoreAsync(mod));

    public Task UninstallModAsync(ModDto mod) => RunAndRefreshAsync(mod, () => UninstallModCoreAsync(mod));

    public Task ToggleModAsync(ModDto mod) => RunAndRefreshAsync(mod, () => ToggleModCoreAsync(mod));

    public async Task InstallModByNameAsync(string name)
    {
        var mod = FindModByName(name);
        if (mod is null)
        {
            Logger.LogWarning("Install requested for unknown mod {Name}", name);
            NotificationService.NoticeLight(Notification_Content_Mod_NotFound(name));
            return;
        }

        if (mod.IsLocal)
        {
            Logger.LogInformation("Install requested for already-installed mod {Name}", name);
            NotificationService.NoticeLight(Notification_Content_Mod_Install_AlreadyInstalled(name));
            return;
        }

        if (mod.State is ModState.Incompatible)
        {
            Logger.LogInformation("Install requested for incompatible mod {Name}", name);
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Incompatible(name));
            return;
        }

        await InstallModAsync(mod).ConfigureAwait(false);
    }

    public async Task UpdateModByNameAsync(string name)
    {
        var mod = FindModByName(name);
        if (mod is null)
        {
            Logger.LogWarning("Update requested for unknown mod {Name}", name);
            NotificationService.NoticeLight(Notification_Content_Mod_NotFound(name));
            return;
        }

        if (!mod.IsLocal)
        {
            Logger.LogInformation("Update requested for not-installed mod {Name}", name);
            NotificationService.ErrorLight(Notification_Content_Mod_NotInstalled(name));
            return;
        }

        if (mod.State is not ModState.Outdated)
        {
            Logger.LogInformation("Update requested for up-to-date mod {Name}", name);
            NotificationService.NoticeLight(Notification_Content_Mod_Update_UpToDate(name));
            return;
        }

        await UpdateModAsync(mod).ConfigureAwait(false);
    }

    public async Task UninstallModByNameAsync(string name)
    {
        var mod = FindModByName(name);
        if (mod is null)
        {
            Logger.LogWarning("Uninstall requested for unknown mod {Name}", name);
            NotificationService.NoticeLight(Notification_Content_Mod_NotFound(name));
            return;
        }

        if (!mod.IsLocal)
        {
            Logger.LogInformation("Uninstall requested for not installed mod {Name}", name);
            NotificationService.NoticeLight(Notification_Content_Mod_NotInstalled(name));
            return;
        }

        await UninstallModAsync(mod).ConfigureAwait(false);
    }

    public async Task<int> UpdateAllModsAsync()
    {
        var outdatedMods = GetOutdatedMods();
        Logger.LogInformation("Updating {ModCount} outdated mod(s)", outdatedMods.Length);

        var updated = 0;
        foreach (var mod in outdatedMods)
        {
            if (await RunExclusiveAsync(mod, () => UpdateModCoreAsync(mod)).ConfigureAwait(false))
            {
                updated++;
            }
        }

        if (updated > 0)
        {
            await RefreshModStatesAsync().ConfigureAwait(false);
        }

        var failed = outdatedMods.Length - updated;
        if (failed > 0)
        {
            NotificationService.WarningLight(Notification_Content_Mod_UpdateAll_Partial(updated, failed));
        }
        else if (updated > 0)
        {
            NotificationService.SuccessLight(Notification_Content_Mod_UpdateAll_Success(updated));
        }

        return outdatedMods.Length;
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IGameDownloadManager GameDownloadManager { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IModLocalService ModLocalService { get; init; }
    public required ILogger<ModManageService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
