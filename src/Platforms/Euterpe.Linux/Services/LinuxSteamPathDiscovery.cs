using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxSteamPathDiscovery : ISteamPathDiscovery
{
    #region Injections

    public required ILogger<LinuxSteamPathDiscovery> Logger { get; init; }

    #endregion Injections

    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder)
    {
        steamFolder = LinuxPaths.SteamSearch.FirstOrDefault(Directory.Exists);
        if (steamFolder is not null && CheckIsValidSteamFolder(steamFolder))
        {
            Logger.LogInformation("Auto detected Steam folder on Linux: {SteamFolder}", steamFolder);
            return true;
        }

        Logger.LogWarning("Auto detect Steam install on common path failed");
        return false;
    }

    public bool CheckIsValidSteamFolder(string folderPath)
    {
        var steamAppsPath = Path.Combine(folderPath, "steamapps");
        if (Directory.Exists(steamAppsPath))
        {
            Logger.LogInformation("Valid Steam folder: {FolderPath}", folderPath);
            return true;
        }

        Logger.LogError("Invalid Steam folder: {FolderPath}", folderPath);
        return false;
    }

    public async Task<string?> GetSteamExecPathAsync()
    {
        try
        {
            var result = await Cli.Wrap("which")
                .WithArguments("steam")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                return null;
            }

            var path = result.StandardOutput.Trim();
            if (path.IsNullOrEmpty() || !File.Exists(path))
            {
                return null;
            }

            Logger.LogInformation("Found Steam executable via 'which': {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run 'which steam'");
            return null;
        }
    }

    public bool CheckIsValidSteamExecPath(string filePath)
    {
        try
        {
            var mode = File.GetUnixFileMode(filePath);
            const UnixFileMode executePermissions = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            var isExecutable = (mode & executePermissions) is not UnixFileMode.None;

            return isExecutable;
        }
        catch
        {
            return false;
        }
    }
}
