namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGamePathDiscovery : IGamePathDiscovery
{
    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder)
    {
        var relativePath = $"steamapps/common/{GameConfig.GameFolderName}";

        if (GamePathService.TryGetGameFolderFromVdf(GameConfig.SteamAppId, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.LogInformation("Could not get game folder from libraryfolders.vdf");

        if (GamePathService.TryGetGameFolderFromCommonPaths(LinuxPaths.SteamSearch, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.LogWarning("Failed to auto detect game path on Linux");
        return false;
    }

    public bool CheckIsValidGameFolder([NotNullWhen(true)] string? folderPath)
    {
        if (folderPath.IsNullOrEmpty())
        {
            return false;
        }

        var exeName = GameConfig.ExecutableName;
        var exePath = Path.Combine(folderPath, exeName);
        var dllPath = Path.Combine(folderPath, "GameAssembly.dll");

        if (!File.Exists(exePath) || !File.Exists(dllPath))
        {
            Logger.LogError("{ExeName} or GameAssembly.dll not found in {FolderPath}", exeName, folderPath);
            return false;
        }

        Logger.LogInformation("{ExeName} and GameAssembly.dll found in {FolderPath}", exeName, folderPath);
        return true;
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IGamePathService GamePathService { get; init; }
    public required ILogger<LinuxGamePathDiscovery> Logger { get; init; }

    #endregion Injections
}
