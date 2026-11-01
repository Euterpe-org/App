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

    private static Config NewConfig() =>
        new() { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() };

    private static SettingPageViewModel NewViewModel(
        IAppLocalService? appLocal = null,
        Func<GameId, IGameLocalService>? gameLocalServiceFor = null,
        Config? config = null)
    {
        config ??= NewConfig();
        return new SettingPageViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            AppLocalService = appLocal ?? IAppLocalService.Mock(),
            GameLocalServiceFor = gameLocalServiceFor ?? (static _ => IGameLocalService.Mock()),
            Config = config,
            LocalizationService = new LocalizationService { Config = config, Logger = Mock.Logger<LocalizationService>() },
            Logger = Mock.Logger<SettingPageViewModel>()
        };
    }
}
