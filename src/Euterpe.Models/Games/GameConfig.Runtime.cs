using Semver;

namespace Euterpe.Models.Games;

public abstract partial class GameConfig
{
    [JsonIgnore]
    public string GameVersion { get; set; } = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    public partial string? MelonLoaderVersion { get; set; }

    [JsonIgnore]
    public SemVersion? MelonLoaderSemVersion { get; private set; }

    [JsonIgnore]
    public string ModsFolder => Path.Combine(Folder, "Mods");

    [JsonIgnore]
    public string UserDataFolder => Path.Combine(Folder, "UserData");

    [JsonIgnore]
    public string UserLibsFolder => Path.Combine(Folder, "UserLibs");

    [JsonIgnore]
    public string GameDataFolder => Path.Combine(Folder, GameDataFolderName);

    [JsonIgnore]
    public string GlobalGameManagersPath => Path.Combine(GameDataFolder, "globalgamemanagers");

    [JsonIgnore]
    public string OnlineChartsFolder => Path.Combine(EuterpeChartsFolder, "Online");

    [JsonIgnore]
    public string OfflineChartsFolder => Path.Combine(EuterpeChartsFolder, "Offline");

    [JsonIgnore]
    public string TempFolder => Path.Combine(Folder, "Euterpe_Temp");

    [JsonIgnore]
    public string TempChartsFolder => Path.Combine(TempFolder, "Charts");

    [JsonIgnore]
    public string TempModsFolder => Path.Combine(TempFolder, "Mods");

    [JsonIgnore]
    public string MelonLoaderFolder => Path.Combine(Folder, "MelonLoader");

    [JsonIgnore]
    public string MelonLoaderZipPath => Path.Combine(Folder, "MelonLoader.zip");

    [JsonIgnore]
    public string LatestLogPath => Path.Combine(MelonLoaderFolder, "Latest.log");

    [JsonIgnore]
    public string DotNetRuntimeFolder => Path.Combine(Folder, "dotnet");

    [JsonIgnore]
    public string DotNetSharedFrameworkFolder => Path.Combine(DotNetRuntimeFolder, "shared", "Microsoft.NETCore.App");

    [JsonIgnore]
    public string MelonLoaderDotNetRuntimeFolder => Path.Combine(MelonLoaderFolder, "Dependencies", "dotnet");

    [JsonIgnore]
    public string MelonLoaderDotNetSharedFrameworkFolder => Path.Combine(MelonLoaderDotNetRuntimeFolder, "shared", "Microsoft.NETCore.App");

    [JsonIgnore]
    public string Cpp2ILExecutablePath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL", "Cpp2IL.exe");

    [JsonIgnore]
    public string Cpp2ILPluginPath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL", "Plugins", "Cpp2IL.Plugin.StrippedCodeRegSupport.dll");

    private string EuterpeChartsFolder => Path.Combine(Folder, "Euterpe_Charts");
    private string Il2CppAssemblyGeneratorFolderPath => Path.Combine(MelonLoaderFolder, "Dependencies", "Il2CppAssemblyGenerator");

    public string UnityDependencyZipPath(string unityVersion) =>
        Path.Combine(Il2CppAssemblyGeneratorFolderPath, $"UnityDependencies_{unityVersion}.zip");

    partial void OnMelonLoaderVersionChanged(string? value) =>
        MelonLoaderSemVersion = SemVersion.TryParse(value, out var version) ? version : null;
}
