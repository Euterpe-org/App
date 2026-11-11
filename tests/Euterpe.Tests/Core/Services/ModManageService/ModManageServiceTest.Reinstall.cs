namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ReinstallModAsync_WhenAllSucceed_ReplacesFileAndNotifiesSuccess()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Never);
        notificationServiceMock.SuccessLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task ReinstallModAsync_WhenDownloadFails_NotifiesError()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Throws(new InvalidOperationException("download failed"));
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Never);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }
}
