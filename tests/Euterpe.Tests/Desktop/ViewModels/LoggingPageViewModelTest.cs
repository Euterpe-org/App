using Euterpe.Features.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("LoggingPageViewModelTests")]
[TestSubject(typeof(LoggingPageViewModel))]
public sealed class LoggingPageViewModelTest
{
    [Test]
    public async Task OpenGameLogCommand_LogExists_RevealsLogFile()
    {
        var root = Directory.CreateTempSubdirectory("euterpe-logging-").FullName;
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            Directory.CreateDirectory(gameConfig.MelonLoaderFolder);
            await File.WriteAllTextAsync(gameConfig.LatestLogPath, string.Empty);
            var launcher = IPlatformLauncher.Mock();
            var vm = NewViewModel(gameConfig, launcher);

            await vm.OpenGameLogCommand.ExecuteAsync(null);

            launcher.RevealFileAsync(gameConfig.LatestLogPath).WasCalled(Times.Once);
            launcher.OpenFolderAsync(Any<string>()).WasCalled(Times.Never);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task OpenGameLogCommand_LogMissing_OpensMelonLoaderFolder()
    {
        var root = Directory.CreateTempSubdirectory("euterpe-logging-").FullName;
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            Directory.CreateDirectory(gameConfig.MelonLoaderFolder);
            var launcher = IPlatformLauncher.Mock();
            var vm = NewViewModel(gameConfig, launcher);

            await vm.OpenGameLogCommand.ExecuteAsync(null);

            launcher.RevealFileAsync(Any<string>()).WasCalled(Times.Never);
            launcher.OpenFolderAsync(gameConfig.MelonLoaderFolder).WasCalled(Times.Once);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task OpenGameLogCommand_MelonLoaderFolderMissing_OpensGameFolder()
    {
        var root = Directory.CreateTempSubdirectory("euterpe-logging-").FullName;
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            var launcher = IPlatformLauncher.Mock();
            var vm = NewViewModel(gameConfig, launcher);

            await vm.OpenGameLogCommand.ExecuteAsync(null);

            launcher.RevealFileAsync(Any<string>()).WasCalled(Times.Never);
            launcher.OpenFolderAsync(gameConfig.Folder).WasCalled(Times.Once);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static LoggingPageViewModel NewViewModel(GameConfig gameConfig, IPlatformLauncher launcher) => new()
    {
        Launcher = launcher,
        Logger = NullLogger<LoggingPageViewModel>.Instance,
        GameConfig = gameConfig,
        Container = null!,
        NavigationService = null!
    };
}
