namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    private async Task HandleModActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["install", var modName]:
                await ModManageService.InstallModByNameAsync(modName).ConfigureAwait(false);
                break;

            case ["update"]:
                await ModManageService.UpdateAllModsAsync().ConfigureAwait(false);
                break;

            case ["update", var modName]:
                await ModManageService.UpdateModByNameAsync(modName).ConfigureAwait(false);
                break;

            case ["uninstall", var modName]:
                await ModManageService.UninstallModByNameAsync(modName).ConfigureAwait(false);
                break;

            default:
                Logger.LogWarning("Unknown mod deep link path: {Path}", path);
                break;
        }
    }
}
