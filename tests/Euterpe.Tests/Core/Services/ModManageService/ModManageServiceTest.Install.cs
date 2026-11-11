namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task InstallModAsync_WhenDownloadSucceeds_AddsLocalInfoAndNotifiesSuccess()
    {
        var mod = CreateInstallableMod();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            notificationService: notificationServiceMock);

        await sut.InstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsTrue();
        await Assert.That(mod.LocalVersion).IsEqualTo("1.0.0");
        await Assert.That(mod.IsDisabled).IsFalse();
        notificationServiceMock.SuccessLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModAsync_WhenDownloadFails_DoesNotAddLocalInfoAndNotifiesError()
    {
        var mod = CreateInstallableMod();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Throws(new InvalidOperationException("download failed"));
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            notificationService: notificationServiceMock);

        await sut.InstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsFalse();
        await Assert.That(mod.LocalVersion).IsEmpty();
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModAsync_WhenConflictsWithAnotherWebMod_MarksTheOtherWebModIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")));
        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        await Assert.That(modB.IsInstallable).IsTrue();

        await sut.InstallModAsync(modA);

        using var _ = Assert.Multiple();
        await Assert.That(modA.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modB.IsInstallable).IsFalse();
    }
}
