using Euterpe.Contracts.Credits;
using Euterpe.Core.Http.Clients;
using Euterpe.Features.Setting;
using Euterpe.Features.Update;
using Ursa.Controls;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("AboutPanelViewModelTests")]
[TestSubject(typeof(AboutPanelViewModel))]
public sealed class AboutPanelViewModelTest
{
    [Test]
    public async Task InitializeAsync_CreditsResponse_MapsContributorGroups()
    {
        var credits = IEuterpeCreditsClient.Mock();
        credits.GetCreditsAsync("zh-CN", Any()).Returns(CreateCreditsResponse());
        var vm = NewViewModel(creditsClient: credits, languageCode: "zh-Hans");

        await vm.InitializeAsync();

        var contributor = vm.ContributorGroups[0].Contributors[0];
        using var assertions = Assert.Multiple();
        await Assert.That(vm.ContributorGroups).HasSingleItem();
        await Assert.That(vm.ContributorGroups[0].GroupName).IsEqualTo("应用程序");
        await Assert.That(contributor.Name).IsEqualTo("Maintainer");
        await Assert.That(contributor.AvatarUrl)
            .IsEqualTo("https://euterpe-org.com/static/images/maintainer.webp");
        await Assert.That(contributor.Links![0].Name).IsEqualTo("GitHub");
        await Assert.That(vm.AllContributorsLoaded).IsTrue();
        credits.GetCreditsAsync("zh-CN", Any()).WasCalled(Times.Once);
    }

    [Test]
    public async Task InitializeAsync_RequestFails_MarksContributorsLoaded()
    {
        var credits = IEuterpeCreditsClient.Mock();
        credits.GetCreditsAsync(Any<string>(), Any()).Throws<HttpRequestException>();
        var vm = NewViewModel(creditsClient: credits);

        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.ContributorGroups).IsEmpty();
        await Assert.That(vm.AllContributorsLoaded).IsTrue();
    }

    [Test]
    public async Task CheckUpdateCommand_NoUpdateFound_ShowsSuccessMessage()
    {
        var update = IUpdateService.Mock();
        update.CheckForUpdatesAsync().Returns((string?)null);
        var msgBox = IMessageBoxService.Mock();
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        msgBox.SuccessAsync(Any<string>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task CheckUpdateCommand_UpdateFound_DoesNotShowSuccess()
    {
        var update = IUpdateService.Mock();
        update.CheckForUpdatesAsync().Returns("2.1.0");
        var msgBox = IMessageBoxService.Mock();
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        msgBox.SuccessAsync(Any<string>()).WasNeverCalled();
        update.UpdateAsync(Any<IProgress<int>>()).WasCalled(Times.Once);
    }

    private static CreditsResponse CreateCreditsResponse() => new(
    [
        new CreditsSection("应用程序",
        [
            new CreditsPerson(
                "Maintainer",
                "/static/images/maintainer.webp",
                "维护项目",
                [new CreditsPersonLink("GitHub", "https://github.com/maintainer")])
        ])
    ]);

    private static AboutPanelViewModel NewViewModel(
        IUpdateService? update = null,
        IMessageBoxService? msgBox = null,
        IEuterpeCreditsClient? creditsClient = null,
        string languageCode = "en")
    {
        var updateService = update ?? IUpdateService.Mock();
        var dialogService = IDialogService.Mock();
        dialogService.ShowOverlayAsync<UpdateDialog, UpdateDialogViewModel>(
            Any<UpdateDialogViewModel>(), Any<OverlayDialogOptions?>(), Any<string?>(), Any<CancellationToken?>());

        return new AboutPanelViewModel
        {
            Config = new Config { LanguageCode = languageCode },
            CreditsClient = creditsClient ?? IEuterpeCreditsClient.Mock(),
            Launcher = IPlatformLauncher.Mock(),
            Logger = Mock.Logger<AboutPanelViewModel>(),
            MessageBoxService = msgBox ?? IMessageBoxService.Mock(),
            UpdateDialogService = new UpdateDialogService
            {
                DialogService = dialogService,
                Logger = Mock.Logger<UpdateDialogService>(),
                MessageBoxService = msgBox ?? IMessageBoxService.Mock(),
                UpdateService = updateService,
                UpdateDialogViewModelFactory = static version => new UpdateDialogViewModel(version)
            },
            UpdateService = updateService
        };
    }
}
