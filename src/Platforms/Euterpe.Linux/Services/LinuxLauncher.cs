using Avalonia.Platform.Storage;

namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed partial class LinuxLauncher : IPlatformLauncher
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
        if (await TryRevealFileAsync(filePath).ConfigureAwait(false))
        {
            Logger.LogInformation("Reveal file: {FilePath}", filePath);
            return;
        }

        var folderPath = Path.GetDirectoryName(filePath)!;
        await OpenFolderAsync(folderPath).ConfigureAwait(false);
    }

    #region Injections

    public required TopLevelProxy TopLevel { get; init; }
    public required ILogger<LinuxLauncher> Logger { get; init; }

    #endregion Injections
}
