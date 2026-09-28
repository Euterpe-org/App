using Euterpe.Features.Modding;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("MelonLoaderPanelViewModelTests")]
[TestSubject(typeof(MelonLoaderPanelViewModel))]
public sealed class MelonLoaderPanelViewModelTest
{
    [Test]
    public async Task OnInitialize_MelonLoaderVersionNull_StatusIsNotInstalled()
    {
        var vm = NewViewModel(new MuseDashConfig { MelonLoaderVersion = null });

        await vm.InitializeAsync();

        await Assert.That(vm.MelonLoaderInstallStatus).IsEqualTo(InstallStatus.NotInstalled);
    }

    [Test]
    public async Task OnInitialize_MelonLoaderVersionPresent_StatusIsInstalled()
    {
        var vm = NewViewModel(new MuseDashConfig { MelonLoaderVersion = "0.6.6" });

        await vm.InitializeAsync();

        await Assert.That(vm.MelonLoaderInstallStatus).IsEqualTo(InstallStatus.Installed);
    }

    private static MelonLoaderPanelViewModel NewViewModel(GameConfig gameConfig) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<MelonLoaderPanelViewModel>.Instance,
        GameConfig = gameConfig,
        DependencyAcquireService = IDependencyAcquireService.Mock(),
        GameLocalService = IGameLocalService.Mock(),
        MessageBoxService = IMessageBoxService.Mock()
    };
}
