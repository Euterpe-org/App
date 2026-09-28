using CliWrap;

namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGameModTemplateInstaller : IGameModTemplateInstaller
{
    public async Task<bool> CheckInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(["new", "list", GameConfig.ModTemplateShortName])
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync()
                .ConfigureAwait(false);

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check Mod Template installation");
            return false;
        }
    }

    public async Task InstallAsync()
    {
        await Cli.Wrap("dotnet")
            .WithArguments(["new", "install", GameConfig.ModTemplatePackageName])
            .ExecuteAsync()
            .ConfigureAwait(false);

        Logger.LogInformation("Mod Template installed successfully");
    }

    public async Task UninstallAsync()
    {
        await Cli.Wrap("dotnet")
            .WithArguments(["new", "uninstall", GameConfig.ModTemplatePackageName])
            .ExecuteAsync()
            .ConfigureAwait(false);

        Logger.LogInformation("Mod Template uninstalled successfully");
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<LinuxGameModTemplateInstaller> Logger { get; init; }

    #endregion Injections
}
