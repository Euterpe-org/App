namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsPlatformInfo : IPlatformInfo
{
    public string OsString => "win";
}
