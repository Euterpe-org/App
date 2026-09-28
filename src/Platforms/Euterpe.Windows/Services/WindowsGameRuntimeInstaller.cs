using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGameRuntimeInstaller : IGameRuntimeInstaller
{
    public async Task<bool> CheckInstalledAsync(string runtimeVersion)
        => CheckGameLocalRuntimeInstalled(runtimeVersion) || await CheckSystemRuntimeInstalledAsync(runtimeVersion).ConfigureAwait(false);

    public async Task InstallAsync(string runtimeVersion)
    {
        var runtimeUrl = DotNetRuntimeDownload.GetUrl(runtimeVersion);
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Logger.LogInformation("Downloading .NET Runtime from {RuntimeUrl} to {TempFilePath}", runtimeUrl, tempFilePath);
            await AppDownloadManager.DownloadFileAsync(runtimeUrl, tempFilePath).ConfigureAwait(false);

            Logger.LogInformation("Extracting .NET Runtime to {RuntimeFolder}", GameConfig.DotNetRuntimeFolder);
            await ArchiveService.ExtractZipFileAsync(tempFilePath, GameConfig.DotNetRuntimeFolder).ConfigureAwait(false);

            Logger.LogInformation(".NET Runtime installed to {RuntimeFolder}", GameConfig.DotNetRuntimeFolder);
        }
        finally
        {
            FileSystemService.TryDeleteFile(tempFilePath);
        }
    }

    private bool CheckGameLocalRuntimeInstalled(string runtimeVersion)
    {
        ReadOnlySpan<string> sharedFrameworkFolders =
        [
            GameConfig.DotNetSharedFrameworkFolder,
            GameConfig.MelonLoaderDotNetSharedFrameworkFolder
        ];

        foreach (var folder in sharedFrameworkFolders)
        {
            if (!ContainsRequiredRuntime(folder, runtimeVersion))
            {
                continue;
            }

            Logger.LogInformation("Game-local .NET {RuntimeVersion} runtime found: {Folder}", runtimeVersion, folder);
            return true;
        }

        Logger.LogInformation("No game-local .NET {RuntimeVersion} runtime found in {GameFolder}", runtimeVersion, GameConfig.Folder);
        return false;
    }

    private static bool ContainsRequiredRuntime(string sharedFrameworkFolder, string runtimeVersion) =>
        Directory.Exists(sharedFrameworkFolder)
        && Directory.EnumerateDirectories(sharedFrameworkFolder, $"{runtimeVersion}.*", SearchOption.TopDirectoryOnly).Any();

    private async Task<bool> CheckSystemRuntimeInstalledAsync(string runtimeVersion)
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-runtimes")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && result.StandardOutput.Contains($"Microsoft.NETCore.App {runtimeVersion}.", StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check .NET runtime installation");
            return false;
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required IArchiveService ArchiveService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<WindowsGameRuntimeInstaller> Logger { get; init; }

    #endregion Injections
}
