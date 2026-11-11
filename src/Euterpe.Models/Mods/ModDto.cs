using Euterpe.Contracts.Mods;

namespace Euterpe.Models.Mods;

public sealed partial class ModDto : ObservableObject
{
    public void RemoveLocalInfo()
    {
        LocalVersion = string.Empty;
        LocalSHA256 = string.Empty;
        DuplicatedModPaths = [];
        FileNameWithoutExtension = null;
        IsDisabled = true;
    }

    public void AddLocalInfo()
    {
        LocalVersion = Version;
        LocalSHA256 = SHA256;
        FileNameWithoutExtension = FileName[..^4];
        IsDisabled = false;
    }

    #region Dto Properties

    // Local Information
    [ObservableProperty]
    public partial string LocalVersion { get; set; } = string.Empty;

    public string LocalSHA256 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInstallable))]
    [NotifyPropertyChangedFor(nameof(IsReinstallable))]
    [NotifyPropertyChangedFor(nameof(ReasonDisplay))]
    public partial ModState State { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReasonDisplay))]
    public partial ModIncompatibleReason IncompatibleReason { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReasonDisplay))]
    public partial string[] ConflictingModNames { get; set; } = [];

    public LocalizedString? ReasonDisplay => State switch
    {
        ModState.Duplicated => XAML.ModState_Duplicated_Reason(string.Join(", ", DuplicatedModPaths)),
        ModState.Incompatible => IncompatibleReason switch
        {
            ModIncompatibleReason.MelonLoader => XAML.ModIncompatibleReason_MelonLoader(MelonVersion),
            ModIncompatibleReason.GameVersion => XAML.ModIncompatibleReason_GameVersion(GameVersion),
            ModIncompatibleReason.ConflictingMod => XAML.ModIncompatibleReason_ConflictingMod(string.Join(", ", ConflictingModNames)),
            _ => null
        },
        _ => null
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocal))]
    [NotifyPropertyChangedFor(nameof(IsInstallable))]
    [NotifyPropertyChangedFor(nameof(IsReinstallable))]
    [NotifyPropertyChangedFor(nameof(IsToggleVisible))]
    public partial string? FileNameWithoutExtension { get; set; }

    public string LocalFileName => FileNameWithoutExtension + (IsDisabled ? ModFiles.DisabledExtension : ModFiles.DllExtension);
    public string ReversedFileName => FileNameWithoutExtension + (IsDisabled ? ModFiles.DllExtension : ModFiles.DisabledExtension);

    // Binding Boolean Properties
    [ObservableProperty]
    public partial bool IsDisabled { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsToggleVisible))]
    public partial bool IsProcessing { get; set; }

    public bool IsLocal => FileNameWithoutExtension is not null;
    public bool HasDownloadSource => !FileName.IsNullOrEmpty();
    public bool IsInstallable => !IsLocal && HasDownloadSource && State is not ModState.Incompatible;
    public bool IsReinstallable => IsLocal && State is ModState.Modified;
    public bool IsToggleVisible => IsLocal && !IsProcessing;

    [ObservableProperty]
    public partial bool IsValidConfigFile { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReasonDisplay))]
    public partial string[] DuplicatedModPaths { get; set; } = [];

    // GitHub Repo
    public string RepoPageUrl => GitHubBaseUrl + Repository;
    public bool HasRepository => !Repository.IsNullOrEmpty();

    // Screenshots
    public bool HasScreenshots => Screenshots is not [];

    // Dependencies
    public bool HasDependency => ModDependencies.Length + LibDependencies.Length > 0;

    public string[] DependencyNames => [.. ModDependencies, .. LibDependencies];

    // Incompatible mods
    public bool HasIncompatibleMods => IncompatibleMods is not [];

    // LocalizedStrings
    public LocalizedString LocalizedCompatibleGameVersion => GameVersion is "*" ? AllGameVersionCompatible : LocalizedString.FromText(GameVersion);

    #endregion Dto Properties

    #region Mod Properties

    public long Mid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "Unknown";
    public string Author { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string GameVersion { get; set; } = "Unknown";
    public string MelonVersion { get; set; } = "Unknown";
    public string Description { get; set; } = string.Empty;
    public string[] ModDependencies { get; set; } = [];
    public string[] LibDependencies { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public ModScreenshot[] Screenshots { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long DownloadCount { get; set; }

    #endregion Mod Properties
}
