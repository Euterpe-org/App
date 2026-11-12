namespace Euterpe.Windows;

internal sealed partial class WindowsLauncher
{
    private bool TryRevealFile(string filePath)
    {
        try
        {
            Process.StartAndForget("explorer.exe", ["/select,", filePath]);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to reveal file through Explorer: {FilePath}", filePath);
            return false;
        }
    }
}
