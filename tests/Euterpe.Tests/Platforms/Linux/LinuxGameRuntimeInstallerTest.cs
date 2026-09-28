using Euterpe.Linux;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Platforms.Linux;

[Category("LinuxGameRuntimeInstallerTests")]
[TestSubject(typeof(LinuxGameRuntimeInstaller))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxGameRuntimeInstallerTest
{
    private const string RuntimeVersion = "8.0";
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"LinuxGameRuntimeInstallerTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [After(Test)]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Test]
    public async Task CheckInstalledAsync_GameLocalRuntimePresent_ReturnsTrue()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(Path.Combine(config.DotNetSharedFrameworkFolder, $"{RuntimeVersion}.36"));

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsTrue();
    }

    [Test]
    public async Task CheckInstalledAsync_DotNetFolderWithoutSharedFramework_ReturnsFalse()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(config.DotNetRuntimeFolder);

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsFalse();
    }

    [Test]
    public async Task CheckInstalledAsync_DifferentMajorVersionOnly_ReturnsFalse()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(Path.Combine(config.DotNetSharedFrameworkFolder, "9.0.0"));

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsFalse();
    }

    [Test]
    public async Task CheckInstalledAsync_DifferentMinorVersionOnly_ReturnsFalse()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(Path.Combine(config.DotNetSharedFrameworkFolder, "8.1.0"));

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsFalse();
    }

    [Test]
    public async Task CheckInstalledAsync_ProtonPrefixRuntimePresent_ReturnsTrue()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        var desktopRuntimeFolder = Path.Combine(
            _tempDir,
            "steamapps", "compatdata", config.SteamAppId, "pfx", "drive_c", "Program Files", "dotnet", "shared", "Microsoft.WindowsDesktop.App");
        Directory.CreateDirectory(Path.Combine(desktopRuntimeFolder, $"{RuntimeVersion}.16"));

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsTrue();
    }

    [Test]
    public async Task CheckInstalledAsync_NothingInstalled_ReturnsFalse()
    {
        var config = new MuseDashConfig { Folder = _tempDir };

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsFalse();
    }

    private LinuxGameRuntimeInstaller CreateInstaller(GameConfig gameConfig) => new()
    {
        Config = new Config { SteamFolder = _tempDir },
        GameConfig = gameConfig,
        AppDownloadManager = IAppDownloadManager.Mock(),
        ArchiveService = IArchiveService.Mock(),
        FileSystemService = IFileSystemService.Mock(),
        Logger = Mock.Logger<LinuxGameRuntimeInstaller>(),
        MessageBoxService = IMessageBoxService.Mock()
    };
}
