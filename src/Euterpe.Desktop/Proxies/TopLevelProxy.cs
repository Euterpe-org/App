using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace Euterpe.Proxies;

public sealed class TopLevelProxy
{
    private TopLevel Current
    {
        get
        {
            var desktop = GetCurrentDesktop();
            return desktop.Windows.LastOrDefault(static w => w.IsVisible)
                   ?? desktop.MainWindow
                   ?? throw new InvalidOperationException("No top-level window available.");
        }
    }

    public IStorageProvider StorageProvider => Current.StorageProvider;
    public ILauncher Launcher => Current.Launcher;
    public IClipboard Clipboard => Current.Clipboard ?? throw new PlatformNotSupportedException("Clipboard is unavailable.");

    #region Injections

    public required ILogger<TopLevelProxy> Logger { get; init; }

    #endregion Injections


    public async Task<string?> TryGetClipboardTextAsync()
    {
        try
        {
            return await Clipboard.TryGetTextAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read the clipboard");
            return null;
        }
    }
}
