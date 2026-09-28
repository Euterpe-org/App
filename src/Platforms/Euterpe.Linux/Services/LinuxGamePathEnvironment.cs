namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGamePathEnvironment : IGamePathEnvironment
{
    public bool IsSet()
    {
        var envValue = Environment.GetEnvironmentVariable(GameConfig.PathEnvironmentVariableName);
        return !envValue.IsNullOrEmpty() && envValue == GameConfig.Folder;
    }

    public bool Set()
    {
        Logger.LogInformation("Ask user to set {VariableName} environment variable to: {GameFolder}", GameConfig.PathEnvironmentVariableName, GameConfig.Folder);
        MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_SetPathEnvironment_Linux, GameConfig.Folder).ConfigureAwait(false);
        return true;
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<LinuxGamePathEnvironment> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
