using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGameRuntimeInstaller : IGameRuntimeInstaller
{
    private const string DllOverrideCommand = """
                                              wine reg add "HKCU\Software\Wine\DllOverrides" /v "version" /t "REG_SZ" /d "native,builtin" /f
                                              """;

    public Task<bool> CheckInstalledAsync(string runtimeVersion) =>
        Task.FromResult(CheckGameLocalRuntimeInstalled(runtimeVersion) || CheckProtonPrefixRuntimeInstalled(runtimeVersion));

    public async Task InstallAsync(string runtimeVersion)
    {
        if (!await CheckProtontricksInstalledAsync().ConfigureAwait(false))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Not_Installed).ConfigureAwait(false);
            throw new InvalidOperationException("Protontricks not installed");
        }

        if (!await ApplyVersionDllOverrideAsync().ConfigureAwait(false))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Wineprefix_Failed).ConfigureAwait(false);
            throw new InvalidOperationException("Failed to configure wineprefix");
        }

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

    private bool CheckProtonPrefixRuntimeInstalled(string runtimeVersion)
    {
        var relativePath = $"steamapps/compatdata/{GameConfig.SteamAppId}/pfx/drive_c/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App";
        var runtimeRoot = Path.Combine(Config.SteamFolder, relativePath);

        if (!Directory.Exists(runtimeRoot))
        {
            Logger.LogInformation(".NET Desktop Runtime root path not found: {RuntimeRoot}", runtimeRoot);
            return false;
        }

        var installed = Directory.EnumerateDirectories(runtimeRoot, $"{runtimeVersion}.*", SearchOption.TopDirectoryOnly).Any();

        if (!installed)
        {
            Logger.LogInformation(".NET Desktop Runtime {RuntimeVersion} not found in {RuntimeRoot}", runtimeVersion, runtimeRoot);
            return false;
        }

        Logger.LogInformation(".NET Desktop Runtime {RuntimeVersion} found in {RuntimeRoot}", runtimeVersion, runtimeRoot);
        return true;
    }

    private async Task<bool> CheckProtontricksInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments("--version")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is 0)
            {
                Logger.LogInformation("Protontricks found: {StandardOutput}", result.StandardOutput.Trim());
                return true;
            }

            Logger.LogError("Protontricks check failed: {StandardError}", result.StandardError);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Protontricks not found");
            return false;
        }
    }

    private async Task<bool> ApplyVersionDllOverrideAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments(["-c", DllOverrideCommand, GameConfig.SteamAppId])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.LogError("Failed to add version dll override: {StandardError}", result.StandardError);
                return false;
            }

            Logger.LogInformation("version dll override added successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to apply version dll override via protontricks");
            return false;
        }
    }

    #region Injections

    public required Config Config { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required IArchiveService ArchiveService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<LinuxGameRuntimeInstaller> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
