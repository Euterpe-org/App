namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    private static string? GetEpkPath(string argument)
    {
        if (Uri.TryCreate(argument, UriKind.Absolute, out var uri))
        {
            return uri.IsFile && uri.LocalPath.EndsWith(ChartFiles.ManifestExtension, StringComparison.OrdinalIgnoreCase)
                ? uri.LocalPath
                : null;
        }

        return argument.EndsWith(ChartFiles.ManifestExtension, StringComparison.OrdinalIgnoreCase) ? argument : null;
    }

    private void HandleEpkFile(string filePath)
    {
        ActivateMainWindow(true);
        HandleEpkFileAsync(filePath).SafeFireAndForget(ex => Logger.LogError(ex, "Failed to open EPK file: {FilePath}", filePath));
    }

    private async Task HandleEpkFileAsync(string filePath)
    {
        Logger.LogInformation("Opening EPK file: {FilePath}", filePath);

        await NavigationService.NavigateToAsync("/charting/toolkit").ConfigureAwait(true);
        await GameScope.Value.Resolve<CharterToolkitPanelViewModel>().OpenEpkAsync(filePath).ConfigureAwait(false);
    }
}
