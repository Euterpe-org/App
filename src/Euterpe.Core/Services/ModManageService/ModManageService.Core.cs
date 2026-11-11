using R3;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task InitializeCoreAsync()
    {
        GameConfig.ObservePropertyChanged(x => x.MelonLoaderVersion, false)
            .SubscribeAwait(this, static (_, self, _) => self.ReconcileAfterMelonLoaderChangeAsync());

        await LoadLibsAsync().ConfigureAwait(false);

        await _reconcileGate.AcquireAsync().ConfigureAwait(false);
        try
        {
            CacheLocalMods(await LoadLocalModsAsync().ConfigureAwait(false));
            await LoadWebModsAsync().ConfigureAwait(false);
            RefreshModStatesCore();
        }
        finally
        {
            _reconcileGate.Release();
        }

        StartWatching();
    }

    private async ValueTask ReconcileAfterMelonLoaderChangeAsync()
    {
        try
        {
            await ReconcileModsAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to reconcile mods after MelonLoader version change");
        }
    }

    private async Task DownloadModCoreAsync(ModDto mod)
    {
        await GameDownloadManager.DownloadModAsync(mod).ConfigureAwait(false);
        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);
        mod.AddLocalInfo();
    }

    private async Task ReplaceModCoreAsync(ModDto mod)
    {
        var previousFileName = mod.LocalFileName;

        await DownloadModCoreAsync(mod).ConfigureAwait(false);

        if (!string.Equals(previousFileName, mod.FileName, StringComparison.OrdinalIgnoreCase))
        {
            FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, previousFileName));
        }
    }

    private async Task InstallModCoreAsync(ModDto mod)
    {
        Logger.LogInformation("Installing mod: {ModName}", mod.Name);

        try
        {
            await DownloadModCoreAsync(mod).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install mod {ModName}", mod.Name);
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Failed(mod.Name));
            return;
        }

        Logger.LogInformation("Mod {ModName} successfully installed", mod.Name);
        NotificationService.SuccessLight(Notification_Content_Mod_Install_Success(mod.Name));
    }

    private async Task<bool> UpdateModCoreAsync(ModDto mod)
    {
        Logger.LogInformation("Updating mod: {ModName} from version {ModLocalVersion} to version {ModVersion}", mod.Name, mod.LocalVersion, mod.Version);

        try
        {
            await ReplaceModCoreAsync(mod).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update mod {ModName}", mod.Name);
            return false;
        }

        Logger.LogInformation("Mod {ModName} successfully updated to version {ModVersion}", mod.Name, mod.Version);
        return true;
    }

    private async Task ReinstallModCoreAsync(ModDto mod)
    {
        Logger.LogInformation("Reinstalling mod: {ModName}", mod.Name);

        try
        {
            await ReplaceModCoreAsync(mod).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to reinstall mod {ModName}", mod.Name);
            NotificationService.ErrorLight(Notification_Content_Mod_Reinstall_Failed(mod.Name));
            return;
        }

        Logger.LogInformation("Mod {ModName} successfully reinstalled", mod.Name);
        NotificationService.SuccessLight(Notification_Content_Mod_Reinstall_Success(mod.Name));
    }

    private async Task UninstallModCoreAsync(ModDto mod)
    {
        Logger.LogInformation("Uninstalling mod: {ModName}", mod.Name);

        if (!FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, mod.LocalFileName)))
        {
            Logger.LogError("Failed to uninstall mod {ModName}: could not delete file {ModLocalFileName}", mod.Name, mod.LocalFileName);
            NotificationService.ErrorLight(Notification_Content_Mod_Uninstall_Failed(mod.Name));
            return;
        }

        await DisableModDependentsAsync(mod).ConfigureAwait(false);
        mod.RemoveLocalInfo();

        Logger.LogInformation("Mod {ModName} successfully uninstalled", mod.Name);
        NotificationService.SuccessLight(Notification_Content_Mod_Uninstall_Success(mod.Name));
    }

    private async Task ToggleModCoreAsync(ModDto mod)
    {
        Logger.LogInformation("Toggling mod: {ModName}", mod.Name);

        var success = mod.IsDisabled
            ? await EnableModAsync(mod).ConfigureAwait(false)
            : await DisableModAsync(mod).ConfigureAwait(false);

        if (!success)
        {
            NotificationService.ErrorLight(Notification_Content_Mod_Toggle_Failed(mod.Name));
        }
    }

    private async Task RunAndRefreshAsync(ModDto mod, Func<Task> action)
    {
        await RunExclusiveAsync(mod, async () =>
        {
            await action().ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);

        await RefreshModStatesAsync().ConfigureAwait(false);
    }

    private async Task<T> RunExclusiveAsync<T>(ModDto mod, Func<Task<T>> action)
    {
        T result = default!;
        await _singleFlight.RunAsync(mod.Name, async () =>
        {
            mod.IsProcessing = true;
            try
            {
                result = await action().ConfigureAwait(false);
            }
            finally
            {
                mod.IsProcessing = false;
            }
        }).ConfigureAwait(false);
        return result;
    }
}
