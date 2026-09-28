using Avalonia.Platform.Storage;

namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed partial class WindowsLauncher : IPlatformLauncher
{
    public async Task OpenFileAsync(string filePath)
    {
        await TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath)).ConfigureAwait(false);
        Logger.LogInformation("Open file: {FilePath}", filePath);
    }

    public async Task OpenFolderAsync(string folderPath)
    {
        await TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath)).ConfigureAwait(false);
        Logger.LogInformation("Open folder: {FolderPath}", folderPath);
    }

    public async Task OpenUriAsync(string uri)
    {
        await TopLevel.Launcher.LaunchUriAsync(new Uri(uri)).ConfigureAwait(false);
        Logger.LogInformation("Open uri: {Uri}", uri);
    }

    public async Task RevealFileAsync(string filePath)
    {
        if (TryRevealFile(filePath))
        {
            Logger.LogInformation("Reveal file: {FilePath}", filePath);
            return;
        }

        var folderPath = Path.GetDirectoryName(filePath)!;
        await OpenFolderAsync(folderPath).ConfigureAwait(false);
    }

    #region Injections

    public required TopLevelProxy TopLevel { get; init; }
    public required ILogger<WindowsLauncher> Logger { get; init; }

    #endregion Injections
}
