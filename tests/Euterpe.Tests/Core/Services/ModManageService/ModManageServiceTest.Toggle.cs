namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ToggleModAsync_WhenDisabled_EnablesMod()
    {
        var mod = CreateInstalledMod(disabled: true);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsFalse();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task ToggleModAsync_WhenEnabled_DisablesMod()
    {
        var mod = CreateInstalledMod(disabled: false);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Never);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ToggleModAsync_WhenMoveFails_DoesNotChangeStateAndNotifiesError(bool initiallyDisabled)
    {
        var mod = CreateInstalledMod(disabled: initiallyDisabled);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(false);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsEqualTo(initiallyDisabled);
        notificationServiceMock.ErrorLight(Any<string>(), Any<TimeSpan?>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task ToggleModAsync_DisablingConflictPartner_ClearsIncompatibleFromBoth()
    {
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
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

        await sut.ToggleModAsync(modB);

        using var _ = Assert.Multiple();
        await Assert.That(modB.IsDisabled).IsTrue();
        await Assert.That(modA.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Normal);
        await Assert.That(modA.IncompatibleReason).IsEqualTo(ModIncompatibleReason.None);
        await Assert.That(modB.IncompatibleReason).IsEqualTo(ModIncompatibleReason.None);
        await Assert.That(modA.ConflictingModNames).IsEmpty();
        await Assert.That(modB.ConflictingModNames).IsEmpty();
    }

    [Test]
    public async Task ToggleModAsync_ReenablingConflictPartner_MarksBothIncompatibleAgain()
    {
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll")),
                ("/mods/ModB.disabled", CreateInstalledMod("ModB", "ModB.dll", true))));
        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        await Assert.That(modA.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Normal);

        await sut.ToggleModAsync(modB);

        using var _ = Assert.Multiple();
        await Assert.That(modB.IsDisabled).IsFalse();
        await Assert.That(modA.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modA.IncompatibleReason).IsEqualTo(ModIncompatibleReason.ConflictingMod);
        await Assert.That(modB.IncompatibleReason).IsEqualTo(ModIncompatibleReason.ConflictingMod);
    }

    [Test]
    public async Task ToggleModAsync_EnableMod_AlsoEnablesDisabledLocalDependency()
    {
        var dep = CreateInstalledMod("ModB", "ModB.dll", true);
        var mod = CreateInstalledMod("ModA", "ModA.dll", true);
        mod.ModDependencies = ["ModB"];

        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/ModA.disabled", mod),
                ("/mods/ModB.disabled", dep)));

        await sut.InitializeModsAsync();
        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsFalse();
        await Assert.That(dep.IsDisabled).IsFalse();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Exactly(2));
    }

    [Test]
    public async Task ToggleModAsync_DisableMod_AlsoDisablesEnabledLocalDependent()
    {
        var dep = CreateInstalledMod("ModB", "ModB.dll");
        var mod = CreateInstalledMod("ModA", "ModA.dll");
        mod.ModDependencies = ["ModB"];

        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", mod),
                ("/mods/ModB.dll", dep)));

        await sut.InitializeModsAsync();
        await sut.ToggleModAsync(dep);

        using var _ = Assert.Multiple();
        await Assert.That(dep.IsDisabled).IsTrue();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Exactly(2));
    }
}
