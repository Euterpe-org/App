using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Euterpe.Windows;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Platforms.Windows;

[Category("WindowsGamePathDiscoveryTests")]
[TestSubject(typeof(WindowsGamePathDiscovery))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsGamePathDiscoveryTest
{
    private readonly MockLogger<WindowsGamePathDiscovery> _logger = Mock.Logger<WindowsGamePathDiscovery>();
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"WindowsGamePathDiscoveryTest_{Guid.NewGuid():N}");
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

    private WindowsGamePathDiscovery CreateService(IGamePathService gamePathService) => new()
    {
        GameConfig = new MuseDashConfig(),
        GamePathService = gamePathService,
        Logger = _logger
    };

    [Test]
    public async Task TryGetGameFolder_VdfReturnsPath_ReturnsTrueWithFolder()
    {
        var expected = Path.Combine(_tempDir, "Muse Dash");
        var pathService = IGamePathService.Mock();
        pathService.TryGetGameFolderFromVdf(Any<string>(), Any<string>())
            .SetsOutGameFolder(expected)
            .Returns(true);

        var service = CreateService(pathService);
        var found = service.TryGetGameFolder(out var folder);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(folder).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetGameFolder_VdfFails_FallsBackToCommonPaths()
    {
        var expected = Path.Combine(_tempDir, "fallback");
        var pathService = IGamePathService.Mock();
        pathService.TryGetGameFolderFromCommonPaths(Any<string[]>(), Any<string>())
            .SetsOutGameFolder(expected)
            .Returns(true);

        var service = CreateService(pathService);
        var found = service.TryGetGameFolder(out var folder);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(folder).IsEqualTo(expected);
        _logger.VerifyLog().ContainingMessage("Could not get game folder from libraryfolders.vdf");
    }

    [Test]
    public async Task TryGetGameFolder_BothMethodsFail_ReturnsFalseAndLogsWarning()
    {
        var service = CreateService(IGamePathService.Mock());
        var found = service.TryGetGameFolder(out var folder);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(folder).IsNull();
        _logger.VerifyLog().ContainingMessage("Failed to auto detect game path on Windows");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task CheckIsValidGameFolder_NullOrEmpty_ReturnsFalse(string? path)
    {
        var service = CreateService(IGamePathService.Mock());
        await Assert.That(service.CheckIsValidGameFolder(path)).IsFalse();
    }

    [Test]
    public async Task CheckIsValidGameFolder_MissingExe_ReturnsFalse()
    {
        await File.WriteAllBytesAsync(Path.Combine(_tempDir, "GameAssembly.dll"), []);

        var service = CreateService(IGamePathService.Mock());
        var result = service.CheckIsValidGameFolder(_tempDir);

        using var _ = Assert.Multiple();
        await Assert.That(result).IsFalse();
        _logger.VerifyLog().ContainingMessage("not found in");
    }

    [Test]
    public async Task CheckIsValidGameFolder_BothFilesPresent_ReturnsTrue()
    {
        var config = new MuseDashConfig();
        await File.WriteAllBytesAsync(Path.Combine(_tempDir, config.ExecutableName), []);
        await File.WriteAllBytesAsync(Path.Combine(_tempDir, "GameAssembly.dll"), []);

        var service = CreateService(IGamePathService.Mock());
        var result = service.CheckIsValidGameFolder(_tempDir);

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        _logger.VerifyLog().ContainingMessage("found in");
    }
}
