namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    public async Task ReconcileModsAsync()
    {
        await _reconcileGate.AcquireAsync().ConfigureAwait(false);
        try
        {
            var localMods = await LoadLocalModsAsync().ConfigureAwait(false);
            var addedMods = FindAddedMods(localMods);
            var removedMods = FindRemovedMods(localMods);

            CacheLocalMods(localMods);
            RemoveLocalMods(removedMods);
            RefreshModStatesCore();
            NotifyModSync(addedMods, removedMods);
        }
        finally
        {
            _reconcileGate.Release();
        }
    }

    private async Task RefreshModStatesAsync()
    {
        await _reconcileGate.AcquireAsync().ConfigureAwait(false);
        try
        {
            RefreshModStatesCore();
        }
        finally
        {
            _reconcileGate.Release();
        }
    }

    private async Task<ModDto[]> LoadLocalModsAsync() =>
    [
        .. (await ModLocalService.GetModFilePaths()
            .WhenAllAsync(ModLocalService.LoadModFromPathAsync).ConfigureAwait(false))
        .OfType<ModDto>()
        .GroupBy(static mod => mod.Name)
        .Select(CheckDuplicatedMod)
    ];

    private static ModDto CheckDuplicatedMod(IGrouping<string, ModDto> localModGroup)
    {
        var localMod = localModGroup.First();
        var fileNames = localModGroup.Select(static mod => mod.LocalFileName).ToArray();
        localMod.DuplicatedModPaths = fileNames.Length > 1 ? fileNames : [];
        return localMod;
    }

    private ModDto[] FindAddedMods(ModDto[] localMods) =>
        localMods.Where(localMod => FindModByName(localMod.Name) is not ({ IsLocal: true } or { IsProcessing: true })).ToArray();

    private ModDto[] FindRemovedMods(ModDto[] localMods)
    {
        var localModNames = localMods.Select(static mod => mod.Name).ToHashSet();
        return GetInstalledMods().Where(mod => !mod.IsProcessing && !localModNames.Contains(mod.Name)).ToArray();
    }

    private void CacheLocalMods(ModDto[] localMods)
    {
        foreach (var localMod in localMods)
        {
            CacheLocalMod(localMod);
        }
    }

    private void CacheLocalMod(ModDto localMod)
    {
        if (_sourceCache.Lookup(localMod.Name) is not { HasValue: true, Value: var cached })
        {
            _sourceCache.AddOrUpdate(localMod);
            return;
        }

        if (cached.IsProcessing)
        {
            return;
        }

        cached.FileNameWithoutExtension = localMod.FileNameWithoutExtension;
        cached.IsDisabled = localMod.IsDisabled;
        cached.LocalVersion = localMod.LocalVersion;
        cached.LocalSHA256 = localMod.LocalSHA256;
        cached.DuplicatedModPaths = localMod.DuplicatedModPaths;
        CheckModFiles(cached);
    }

    private void RemoveLocalMods(ModDto[] removedMods)
    {
        foreach (var removedMod in removedMods)
        {
            RemoveLocalMod(removedMod);
        }
    }

    private void RemoveLocalMod(ModDto removedMod)
    {
        if (removedMod.HasDownloadSource)
        {
            removedMod.RemoveLocalInfo();
        }
        else
        {
            _sourceCache.RemoveKey(removedMod.Name);
        }
    }

    private void RefreshModStatesCore()
    {
        var enabledMods = GetEnabledMods();
        foreach (var mod in _sourceCache.Items.Where(static mod => !mod.IsProcessing))
        {
            mod.ConflictingModNames = FindConflictingMods(mod, enabledMods);
            mod.IncompatibleReason = DetermineIncompatibleReason(mod);
            mod.State = DetermineModState(mod);
        }

        _sourceCache.Refresh();
    }

    private static string[] FindConflictingMods(ModDto mod, ModDto[] enabledMods)
    {
        if (mod is { IsLocal: true, IsDisabled: true })
        {
            return [];
        }

        return enabledMods
            .Where(other => mod.IncompatibleMods.Contains(other.Name) || other.IncompatibleMods.Contains(mod.Name))
            .Select(static other => other.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private ModIncompatibleReason DetermineIncompatibleReason(ModDto mod)
    {
        if (mod.ConflictingModNames is not [])
        {
            return ModIncompatibleReason.ConflictingMod;
        }

        if (!mod.HasDownloadSource)
        {
            return ModIncompatibleReason.None;
        }

        if (GameConfig.MelonLoaderSemVersion is { } semVersion
            && SemVersionRange.TryParse($"^{mod.MelonVersion}", out var range)
            && !range.Contains(semVersion))
        {
            return ModIncompatibleReason.MelonLoader;
        }

        return mod.GameVersion is not "*" && mod.GameVersion != GameConfig.GameVersion
            ? ModIncompatibleReason.GameVersion
            : ModIncompatibleReason.None;
    }

    private static ModState DetermineModState(ModDto mod)
    {
        if (mod.DuplicatedModPaths is not [])
        {
            return ModState.Duplicated;
        }

        if (mod.IncompatibleReason is not ModIncompatibleReason.None)
        {
            return ModState.Incompatible;
        }

        if (!mod.IsLocal || !mod.HasDownloadSource)
        {
            return ModState.Normal;
        }

        return mod.LocalVersion.ComparePrecedenceTo(mod.Version) switch
        {
            < 0 => ModState.Outdated,
            > 0 => ModState.Newer,
            _ when mod.LocalSHA256 != mod.SHA256 => ModState.Modified,
            _ => ModState.Normal
        };
    }

    private void NotifyModSync(ModDto[] addedMods, ModDto[] removedMods)
    {
        switch (addedMods, removedMods)
        {
            case ([], []):
                break;
            case ([var addedMod], []):
                NotificationService.SuccessLight(Notification_Content_Mod_Sync_Added(addedMod.Name));
                break;
            case ([], [var removedMod]):
                NotificationService.NoticeLight(Notification_Content_Mod_Sync_Removed(removedMod.Name));
                break;
            default:
                NotificationService.NoticeLight(Notification_Content_Mod_Sync_Summary(addedMods.Length, removedMods.Length));
                break;
        }
    }
}
