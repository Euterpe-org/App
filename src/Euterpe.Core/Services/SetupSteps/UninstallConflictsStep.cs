namespace Euterpe.Core;

internal sealed class UninstallConflictsStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.UninstallConflicts;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);

        var mod = ModManageService.FindModByName(AppName);
        if (mod is null)
        {
            return;
        }

        foreach (var modName in mod.IncompatibleMods)
        {
            var incompatibleMod = ModManageService.FindModByName(modName);
            if (incompatibleMod is not { IsLocal: true })
            {
                continue;
            }

            Logger.LogInformation("Conflict detected: {ModName} is installed and will be uninstalled", modName);
            progress.Report(XAML.Setup_Progress_UninstallingConflict(modName));
            await ModManageService.UninstallModAsync(incompatibleMod).ConfigureAwait(false);
        }
    }

    #region Injections

    public required IModManageService ModManageService { get; init; }
    public required ILogger<UninstallConflictsStep> Logger { get; init; }

    #endregion Injections
}
