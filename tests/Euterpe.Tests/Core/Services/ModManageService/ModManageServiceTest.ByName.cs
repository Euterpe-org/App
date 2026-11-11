namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task InstallModByNameAsync_WhenModUnknown_NotifiesAndDoesNotDownload()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(gameDownloadManager: downloadManagerMock, notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.InstallModByNameAsync("Unknown");

        using var _ = Assert.Multiple();
        notificationServiceMock.NoticeLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModByNameAsync_WhenAlreadyInstalled_NotifiesAndDoesNotDownload()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())),
            notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.InstallModByNameAsync(TestModName);

        using var _ = Assert.Multiple();
        notificationServiceMock.NoticeLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModByNameAsync_WhenInstallable_Downloads()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod()]);
        var sut = CreateModManageService(gameDownloadManager: downloadManagerMock);
        await sut.InitializeModsAsync();

        await sut.InstallModByNameAsync(TestModName);

        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task InstallModByNameAsync_WhenIncompatible_NotifiesAndDoesNotDownload()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod(melonVersion: "0.5.0")]);
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(
            CreateGame(melonLoaderVersion: "0.4.0"),
            downloadManagerMock,
            notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.InstallModByNameAsync(TestModName);

        using var _ = Assert.Multiple();
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModByNameAsync_WhenConflictsWithInstalledMod_NotifiesAndDoesNotDownload()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([
            CreateWebMod("ModA", incompatibleMods: ["ModB"]),
            CreateWebMod("ModB")
        ]);
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            modLocalService: LocalServiceWith(("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll"))),
            notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.InstallModByNameAsync("ModB");

        using var _ = Assert.Multiple();
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateModByNameAsync_WhenNotInstalled_NotifiesAndDoesNotDownload()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod()]);
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(gameDownloadManager: downloadManagerMock, notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.UpdateModByNameAsync(TestModName);

        using var _ = Assert.Multiple();
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateModByNameAsync_WhenNotOutdated_NotifiesAndDoesNotDownload()
    {
        var upToDate = CreateInstalledMod();
        upToDate.SHA256 = "shared-sha";
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod(sha256: "shared-sha")]);
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            modLocalService: LocalServiceWith((TestModFilePath, upToDate)),
            notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.UpdateModByNameAsync(TestModName);

        using var _ = Assert.Multiple();
        notificationServiceMock.NoticeLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateModByNameAsync_WhenOutdated_Downloads()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod(version: "2.0.0")]);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));
        await sut.InitializeModsAsync();

        await sut.UpdateModByNameAsync(TestModName);

        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task UninstallModByNameAsync_WhenNotInstalled_NotifiesAndDoesNotDelete()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod()]);
        var fileSystemServiceMock = IFileSystemService.Mock();
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        await sut.UninstallModByNameAsync(TestModName);

        using var _ = Assert.Multiple();
        notificationServiceMock.NoticeLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UninstallModByNameAsync_WhenInstalled_Deletes()
    {
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));
        await sut.InitializeModsAsync();

        await sut.UninstallModByNameAsync(TestModName);

        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Once);
    }
}
