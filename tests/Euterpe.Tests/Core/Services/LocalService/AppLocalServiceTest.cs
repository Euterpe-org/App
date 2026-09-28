using Avalonia.Platform.Storage;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("AppLocalServiceTests")]
[TestSubject(typeof(AppLocalService))]
public sealed class AppLocalServiceTest
{
    private readonly MockLogger<AppLocalService> _logger = Mock.Logger<AppLocalService>();

    private AppLocalService CreateService(
        StubFileSystemPickerService picker,
        ISteamPathDiscovery? steam = null,
        IMessageBoxService? messageBox = null) => new()
    {
        FileSystemPickerService = picker,
        Logger = _logger,
        MessageBoxService = messageBox ?? IMessageBoxService.Mock(),
        SteamDiscovery = steam ?? ISteamPathDiscovery.Mock()
    };

    [Test]
    public async Task GetSteamFolderAsync_FirstPickValid_ReturnsImmediately()
    {
        var picker = new StubFileSystemPickerService { FolderResults = ["/steam"] };

        var steam = ISteamPathDiscovery.Mock();
        steam.CheckIsValidSteamFolder("/steam").Returns(true);

        var service = CreateService(picker, steam);
        var result = await service.GetSteamFolderAsync();

        await Assert.That(result).IsEqualTo("/steam");
    }

    [Test]
    public async Task GetSteamFolderAsync_FirstPickInvalid_RetriesAfterErrorMessage()
    {
        var picker = new StubFileSystemPickerService { FolderResults = ["/bad", "/good"] };

        var steam = ISteamPathDiscovery.Mock();
        steam.CheckIsValidSteamFolder("/bad").Returns(false);
        steam.CheckIsValidSteamFolder("/good").Returns(true);

        var messageBox = IMessageBoxService.Mock();
        messageBox.ErrorAsync(Any<string>()).Returns(() => Task.CompletedTask);

        var service = CreateService(picker, steam, messageBox);
        var result = await service.GetSteamFolderAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsEqualTo("/good");
        messageBox.ErrorAsync(Any<string>()).WasCalled(Times.AtLeastOnce);
    }

    [Test]
    public async Task GetSteamExecPathAsync_FirstPickValid_ReturnsImmediately()
    {
        var picker = new StubFileSystemPickerService { FileResults = ["/usr/bin/steam"] };

        var steam = ISteamPathDiscovery.Mock();
        steam.CheckIsValidSteamExecPath("/usr/bin/steam").Returns(true);

        var service = CreateService(picker, steam);
        var result = await service.GetSteamExecPathAsync();

        await Assert.That(result).IsEqualTo("/usr/bin/steam");
    }

    [Test]
    public async Task GetSteamExecPathAsync_FirstPickInvalid_RetriesAfterErrorMessage()
    {
        var picker = new StubFileSystemPickerService { FileResults = ["/bad-exec", "/usr/bin/steam"] };

        var steam = ISteamPathDiscovery.Mock();
        steam.CheckIsValidSteamExecPath("/bad-exec").Returns(false);
        steam.CheckIsValidSteamExecPath("/usr/bin/steam").Returns(true);

        var messageBox = IMessageBoxService.Mock();
        messageBox.ErrorAsync(Any<string>()).Returns(() => Task.CompletedTask);

        var service = CreateService(picker, steam, messageBox);
        var result = await service.GetSteamExecPathAsync();

        await Assert.That(result).IsEqualTo("/usr/bin/steam");
    }

    [Test]
    public async Task GetCacheFolderAsync_FirstNonEmptyPick_Returns()
    {
        var picker = new StubFileSystemPickerService { FolderResults = ["/cache"] };

        var service = CreateService(picker);
        var result = await service.GetCacheFolderAsync();

        await Assert.That(result).IsEqualTo("/cache");
    }

    [Test]
    public async Task GetCacheFolderAsync_FirstEmpty_RetriesUntilNonEmpty()
    {
        var picker = new StubFileSystemPickerService { FolderResults = ["", "/cache"] };

        var service = CreateService(picker);
        var result = await service.GetCacheFolderAsync();

        await Assert.That(result).IsEqualTo("/cache");
    }

    private sealed class StubFileSystemPickerService : IFileSystemPickerService
    {
        private int _fileIndex;
        private int _folderIndex;

        public string?[] FolderResults { get; init; } = [];
        public string?[] FileResults { get; init; } = [];

        public Task<string?> GetSingleFolderPathAsync(string dialogTitle) =>
            Task.FromResult(_folderIndex < FolderResults.Length ? FolderResults[_folderIndex++] : null);

        public Task<string?> GetSingleFilePathAsync(string dialogTitle) =>
            Task.FromResult(_fileIndex < FileResults.Length ? FileResults[_fileIndex++] : null);

        public Task<IStorageFolder?> GetSingleFolderAsync(string dialogTitle) => throw new NotSupportedException();
        public Task<IReadOnlyList<IStorageFolder>> GetMultipleFoldersAsync(string dialogTitle) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> GetMultipleFolderPathsAsync(string dialogTitle) => throw new NotSupportedException();
        public Task<IStorageFile?> GetSingleFileAsync(string dialogTitle) => throw new NotSupportedException();

        public Task<IReadOnlyList<IStorageFile>> GetMultipleFilesAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? fileTypes = null) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<string>> GetMultipleFilePathsAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? fileTypes = null) =>
            throw new NotSupportedException();
    }
}
