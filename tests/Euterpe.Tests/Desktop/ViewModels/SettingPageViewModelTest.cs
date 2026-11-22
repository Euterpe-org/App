using Euterpe.Features.Setting;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("SettingPageViewModelTests")]
[TestSubject(typeof(SettingPageViewModel))]
public sealed class SettingPageViewModelTest
{
    [Test]
    public async Task ChangeGameFolderCommand_FolderPicked_WritesPathToThatGamesConfig()
    {
        var gameLocal = IGameLocalService.Mock();
        gameLocal.GetGameFolderAsync().Returns("/games/musedash");
        var gameConfig = new MuseDashConfig { Folder = "/old" };
        GameId? requestedGame = null;
        var vm = NewViewModel(gameLocalServiceFor: game =>
        {
            requestedGame = game;
            return gameLocal;
        });

        await vm.ChangeGameFolderCommand.ExecuteAsync(gameConfig);

        using var _ = Assert.Multiple();
        await Assert.That(requestedGame).IsEqualTo(GameId.MuseDash);
        await Assert.That(gameConfig.Folder).IsEqualTo("/games/musedash");
    }

    [Test]
    public async Task ChangeCacheFolderCommand_FolderPicked_WritesPathToConfig()
    {
        var appLocal = IAppLocalService.Mock();
        appLocal.GetCacheFolderAsync().Returns("/cache/new");
        var config = NewConfig();
        config.CacheFolder = "/cache/old";
        var vm = NewViewModel(appLocal, config: config);

        await vm.ChangeCacheFolderCommand.ExecuteAsync(null);

        await Assert.That(config.CacheFolder).IsEqualTo("/cache/new");
    }

    [Test]
    public async Task LogoutCommand_Confirmed_LogsOutAndShutsDown()
    {
        var messageBox = IMessageBoxService.Mock();
        messageBox.WarningConfirmAsync(Any<string>()).Returns(true);
        var auth = IAuthService.Mock();
        var appLifetime = IAppLifetimeService.Mock();
        var vm = NewViewModel(auth: auth, messageBox: messageBox, appLifetime: appLifetime);

        await vm.LogoutCommand.ExecuteAsync(null);

        auth.LogoutAsync().WasCalled(Times.Once);
        appLifetime.Shutdown().WasCalled(Times.Once);
    }

    [Test]
    public async Task LogoutCommand_Declined_StaysLoggedIn()
    {
        var messageBox = IMessageBoxService.Mock();
        messageBox.WarningConfirmAsync(Any<string>()).Returns(false);
        var auth = IAuthService.Mock();
        var appLifetime = IAppLifetimeService.Mock();
        var vm = NewViewModel(auth: auth, messageBox: messageBox, appLifetime: appLifetime);

        await vm.LogoutCommand.ExecuteAsync(null);

        auth.LogoutAsync().WasCalled(Times.Never);
        appLifetime.Shutdown().WasCalled(Times.Never);
    }

    private static Config NewConfig() =>
        new() { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() };

    private static SettingPageViewModel NewViewModel(
        IAppLocalService? appLocal = null,
        Func<GameId, IGameLocalService>? gameLocalServiceFor = null,
        Config? config = null,
        IAuthService? auth = null,
        IMessageBoxService? messageBox = null,
        IAppLifetimeService? appLifetime = null)
    {
        config ??= NewConfig();
        return new SettingPageViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            AppLocalService = appLocal ?? IAppLocalService.Mock(),
            GameLocalServiceFor = gameLocalServiceFor ?? (static _ => IGameLocalService.Mock()),
            Config = config,
            AuthState = new AuthState(),
            AuthService = auth ?? IAuthService.Mock(),
            MessageBoxService = messageBox ?? IMessageBoxService.Mock(),
            AppLifetimeService = appLifetime ?? IAppLifetimeService.Mock(),
            LocalizationService = new LocalizationService { Config = config, Logger = Mock.Logger<LocalizationService>() },
            Logger = Mock.Logger<SettingPageViewModel>()
        };
    }
}
