namespace Euterpe.Windows;

internal sealed partial class WindowsLauncher
{
    private bool TryRevealFile(string filePath)
    {
        try
        {
            using var process = Process.Start(
                new ProcessStartInfo("explorer.exe")
                {
                    ArgumentList = { "/select,", filePath },
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            );
            return process is not null;
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to reveal file through Explorer: {FilePath}", filePath);
            return false;
        }
    }
}
