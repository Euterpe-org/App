namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxPlatformInfo : IPlatformInfo
{
    public string OsString => "linux";
}
