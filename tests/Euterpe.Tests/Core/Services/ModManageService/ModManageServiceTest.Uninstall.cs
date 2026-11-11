namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task UninstallModAsync_WhenDeleteSucceeds_RemovesLocalInfoAndNotifiesSuccess()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UninstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsFalse();
        await Assert.That(mod.LocalVersion).IsEmpty();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UninstallModAsync_WhenDeleteFails_KeepsLocalInfoAndNotifiesError()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(false);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UninstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsTrue();
        await Assert.That(mod.LocalVersion).IsEqualTo("1.0.0");
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UninstallModAsync_WhenModIncompatibleWithMelonLoader_RemainsIncompatible()
    {
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var sut = CreateModManageService(
            CreateGame(melonLoaderVersion: "0.4.0"),
            DownloadManagerWith(CreateWebMod(melonVersion: "0.5.0")),
            fileSystemServiceMock,
            LocalServiceWith((TestModFilePath, CreateInstalledMod())));
        await sut.InitializeModsAsync();

        var mod = sut.FindModByName(TestModName)!;
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);

        await sut.UninstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsFalse();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(mod.IsInstallable).IsFalse();
    }

    [Test]
    public async Task UninstallModAsync_WhenConflictCleared_RestoresOutdatedPartner()
    {
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", "2.0.0", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll")),
                ("/mods/ModB.dll", CreateInstalledMod("ModB", "ModB.dll"))));
        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        await Assert.That(modA.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);

        await sut.UninstallModAsync(modB);

        using var _ = Assert.Multiple();
        await Assert.That(modA.State).IsEqualTo(ModState.Outdated);
        await Assert.That(modB.IsLocal).IsFalse();
    }

    [Test]
    public async Task UninstallModAsync_WhenConflictSourceRemoved_RestoresWebPeerInstallable()
    {
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll"))));
        await sut.InitializeModsAsync();

        var modB = sut.FindModByName("ModB")!;
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);

        await sut.UninstallModAsync(sut.FindModByName("ModA")!);

        using var _ = Assert.Multiple();
        await Assert.That(modB.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.IsInstallable).IsTrue();
    }
}
