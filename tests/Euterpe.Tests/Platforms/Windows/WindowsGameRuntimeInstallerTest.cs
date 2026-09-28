using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Downloader;
using Euterpe.Windows;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Platforms.Windows;

[Category("WindowsGameRuntimeInstallerTests")]
[TestSubject(typeof(WindowsGameRuntimeInstaller))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsGameRuntimeInstallerTest
{
    private const string RuntimeVersion = "8.0";
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"WindowsGameRuntimeInstallerTest_{Guid.NewGuid():N}");
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
    public async Task CheckInstalledAsync_MelonLoaderLocalRuntimePresent_ReturnsTrue()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(Path.Combine(config.MelonLoaderDotNetSharedFrameworkFolder, $"{RuntimeVersion}.16"));

        var installed = await CreateInstaller(config).CheckInstalledAsync(RuntimeVersion);

        await Assert.That(installed).IsTrue();
    }

    [Test]
    public async Task InstallAsync_RuntimeVersion_DownloadsMatchingRuntime()
    {
        var downloadManager = IAppDownloadManager.Mock();
        var installer = CreateInstaller(new MuseDashConfig { Folder = _tempDir }, downloadManager);

        await installer.InstallAsync(RuntimeVersion);

        downloadManager.DownloadFileAsync(
                "https://aka.ms/dotnet/8.0/dotnet-runtime-win-x64.zip",
                Any<string>(),
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Once);
    }

    private static WindowsGameRuntimeInstaller CreateInstaller(
        GameConfig gameConfig,
        IAppDownloadManager? downloadManager = null) => new()
    {
        GameConfig = gameConfig,
        AppDownloadManager = downloadManager ?? IAppDownloadManager.Mock(),
        ArchiveService = IArchiveService.Mock(),
        FileSystemService = IFileSystemService.Mock(),
        Logger = Mock.Logger<WindowsGameRuntimeInstaller>()
    };
}
