using System.Runtime.InteropServices;
using Cake.Common.Build;
using static Cake.Common.ArgumentAliases;
using static Euterpe.Shared.BuildInfo;

namespace Euterpe.Releaser;

public sealed partial class ReleaseContext : FrostingContext
{
    public const string WindowsPackageIconPath = "src/Euterpe.Desktop/Assets/Icon.ico";
    public const string LinuxPackageIconPath = "src/Euterpe.Desktop/Assets/Icon.png";
    public const string PackageId = "Euterpe";

    public string ApplicationProject { get; }
    public string Rid { get; }
    public SemVersion Version { get; init; }
    public string InstallerFileSuffix { get; }
    public IReadOnlyList<string> PlatformVpkArguments { get; }

    public ReleaseContext(ICakeContext context) : base(context)
    {
        Rid = context.Argument("rid", RuntimeInformation.RuntimeIdentifier);
        Version = SemVersion.Parse(AppVersion, SemVersionStyles.Strict);
        var rid = Rid.AsSpan();
        var separatorIndex = rid.IndexOf('-');
        switch (separatorIndex < 0 ? rid : rid[..separatorIndex])
        {
            case "win":
                ApplicationProject = "src/Platforms/Euterpe.Windows/Euterpe.Windows.csproj";
                InstallerFileSuffix = "-Setup.exe";
                PlatformVpkArguments = ["--noPortable", "--icon", WindowsPackageIconPath];
                break;
            case "linux":
                ApplicationProject = "src/Platforms/Euterpe.Linux/Euterpe.Linux.csproj";
                InstallerFileSuffix = ".AppImage";
                PlatformVpkArguments = ["--icon", LinuxPackageIconPath];
                break;
            default:
                throw new InvalidOperationException($"Unsupported release RID: {Rid}");
        }
    }

    public void EnsureGitHubActions()
    {
        if (!this.GitHubActions().IsRunningOnGitHubActions)
        {
            throw new InvalidOperationException("Remote release tasks can only run in GitHub Actions");
        }
    }
}
