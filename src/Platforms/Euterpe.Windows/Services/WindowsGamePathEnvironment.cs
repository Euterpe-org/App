namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGamePathEnvironment : IGamePathEnvironment
{
    public bool IsSet()
    {
        var envValue = Environment.GetEnvironmentVariable(GameConfig.PathEnvironmentVariableName);
        return !envValue.IsNullOrEmpty() && envValue == GameConfig.Folder;
    }

    public bool Set()
    {
        try
        {
            Logger.LogInformation("Set {VariableName} environment variable to: {GameFolder}", GameConfig.PathEnvironmentVariableName, GameConfig.Folder);
            Environment.SetEnvironmentVariable(GameConfig.PathEnvironmentVariableName, GameConfig.Folder, EnvironmentVariableTarget.User);
            MessageBoxService.SuccessOverlayAsync(MessageBox_Content_SetPathEnvironment_Windows, GameConfig.Folder).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set {VariableName} environment variable", GameConfig.PathEnvironmentVariableName);
            return false;
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<WindowsGamePathEnvironment> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
