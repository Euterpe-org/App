using Euterpe.Linux.DBus;
using Tmds.DBus.Protocol;

namespace Euterpe.Linux;

internal sealed partial class LinuxLauncher
{
    private const string FileManagerServiceName = "org.freedesktop.FileManager1";
    private const string FileManagerObjectPath = "/org/freedesktop/FileManager1";

    private async Task<bool> TryRevealFileAsync(string filePath)
    {
        try
        {
            var service = new DBusService(DBusConnection.Session, FileManagerServiceName);
            var fileManager = service.CreateFileManager1(FileManagerObjectPath);
            var fileUri = new Uri(filePath).AbsoluteUri;

            await fileManager.ShowItemsAsync([fileUri], string.Empty).ConfigureAwait(false);
            return true;
        }
        catch (DBusExceptionBase ex)
        {
            Logger.LogDebug(ex, "Failed to reveal file through D-Bus: {FilePath}", filePath);
            return false;
        }
    }
}
