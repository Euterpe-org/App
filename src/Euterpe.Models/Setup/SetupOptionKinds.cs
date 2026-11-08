namespace Euterpe.Models.Setup;

[Flags]
public enum SetupOptionKinds
{
    None = 0,
    MelonLoader = 1 << 0,
    DotNetRuntime = 1 << 1,
    EssentialMods = 1 << 2,
    UninstallConflicts = 1 << 3,
    ChartingTool = 1 << 5,
    DotNetSdk = 1 << 6,
    ModTemplate = 1 << 7,
    EnvVariable = 1 << 8
}
