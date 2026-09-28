using System.Text;
using Euterpe.Features.Logging;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("MelonLoaderLogPanelViewModelTests")]
[TestSubject(typeof(MelonLoaderLogPanelViewModel))]
public sealed class MelonLoaderLogPanelViewModelTest
{
    [Test]
    public async Task InitializeAsync_LogAvailable_LoadsLinesOnce()
    {
        var fileSystem = IFileSystemService.Mock();
        var vm = NewViewModel(fileSystem);
        fileSystem.TryOpenSharedReadFile(vm.GameConfig.LatestLogPath)
            .Returns(() => LogStream("first\r\nsecond\n\nthird\r\n"));

        await vm.InitializeAsync();
        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.LogLines)
            .IsEquivalentTo(["first", "second", "third"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(vm.HasLogLines).IsTrue();
        fileSystem.TryOpenSharedReadFile(vm.GameConfig.LatestLogPath).WasCalled(Times.Once);
    }

    [Test]
    public async Task InitializeAsync_LogUnavailable_ShowsEmptyState()
    {
        var fileSystem = IFileSystemService.Mock();
        var vm = NewViewModel(fileSystem);
        fileSystem.TryOpenSharedReadFile(vm.GameConfig.LatestLogPath).Returns((Stream?)null);

        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.LogLines).IsEmpty();
        await Assert.That(vm.HasLogLines).IsFalse();
    }

    [Test]
    public async Task RefreshLogCommand_LogChanges_ReplacesLines()
    {
        var fileSystem = IFileSystemService.Mock();
        var vm = NewViewModel(fileSystem);
        var content = "before";
        fileSystem.TryOpenSharedReadFile(vm.GameConfig.LatestLogPath)
            .Returns(() => LogStream(content));

        await vm.InitializeAsync();
        content = "after\nupdated";

        await vm.RefreshLogCommand.ExecuteAsync(null);

        using var assertions = Assert.Multiple();
        await Assert.That(vm.LogLines)
            .IsEquivalentTo(["after", "updated"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
        fileSystem.TryOpenSharedReadFile(vm.GameConfig.LatestLogPath).WasCalled(Times.Exactly(2));
    }

    private static MemoryStream LogStream(string content) => new(Encoding.UTF8.GetBytes(content));

    private static MelonLoaderLogPanelViewModel NewViewModel(IFileSystemService fileSystem) => new()
    {
        FileSystemService = fileSystem,
        GameConfig = new MuseDashConfig { Folder = "C:\\Games\\Muse Dash" },
        Launcher = IPlatformLauncher.Mock(),
        Logger = Mock.Logger<MelonLoaderLogPanelViewModel>()
    };
}
