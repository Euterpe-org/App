namespace Euterpe.Models.Games;

public sealed class MuseDashConfig : GameConfig
{
    private const SetupOptionKinds Required =
        SetupOptionKinds.MelonLoader
        | SetupOptionKinds.DotNetRuntime
        | SetupOptionKinds.EssentialMods
        | SetupOptionKinds.UninstallConflicts;

    [JsonIgnore]
    public override GameId Id => GameId.MuseDash;

    [JsonIgnore]
    public override string DisplayName => "Muse Dash";

    [JsonIgnore]
    public override string SteamAppId => "774171";

    [JsonIgnore]
    public override string ExecutableName => "MuseDash.exe";

    [JsonIgnore]
    public override string GameFolderName => "Muse Dash";

    [JsonIgnore]
    public override string GameDataFolderName => "MuseDash_Data";

    [JsonIgnore]
    public override string ModTemplatePackageName => "MuseDash.Mod.Template";

    [JsonIgnore]
    public override string ModTemplateShortName => "musedashmod";

    [JsonIgnore]
    public override string PathEnvironmentVariableName => "MD_DIRECTORY";

    [JsonIgnore]
    public override IReadOnlyList<SetupOption> SetupOptions { get; } =
    [
        new(SetupOptionKinds.MelonLoader, Setup_Task_MelonLoader, Setup_Task_MelonLoader_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.DotNetRuntime, Setup_Task_DotNetRuntime, Setup_Task_DotNetRuntime_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.EssentialMods, Setup_Task_EssentialMods, Setup_Task_EssentialMods_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.UninstallConflicts, Setup_Task_UninstallConflicts, Setup_Task_UninstallConflicts_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.ChartingTool, Setup_Task_ChartingTool, Setup_Task_ChartingTool_Description),
        new(SetupOptionKinds.DotNetSdk, Setup_Task_DotNetSdk, Setup_Task_DotNetSdk_Description),
        new(SetupOptionKinds.ModTemplate, Setup_Task_ModTemplate, Setup_Task_ModTemplate_Description),
        new(SetupOptionKinds.EnvVariable, Setup_Task_EnvVariable, Setup_Task_EnvVariable_Description)
    ];

    [JsonIgnore]
    public override IReadOnlyDictionary<WizardIdentity, SetupOptionKinds> WizardPresets { get; } = new Dictionary<WizardIdentity, SetupOptionKinds>
    {
        [WizardIdentity.Player] = Required,
        [WizardIdentity.Charter] = Required
                                   | SetupOptionKinds.ChartingTool,
        [WizardIdentity.Modder] = Required
                                  | SetupOptionKinds.DotNetSdk
                                  | SetupOptionKinds.ModTemplate
                                  | SetupOptionKinds.EnvVariable
    };
}
