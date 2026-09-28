namespace Euterpe.Features.Logging;

[Route("/logging", DisplayName = Page_Logging, Icon = "Terminal", Order = 3)]
public sealed partial class LoggingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new DropDownButtonItem(DropDownButton_Open,
        [
            new DropDownMenuItem(File_AppLogs, RevealFileCommand, LogFilePath),
            new DropDownMenuItem(File_GameLogs, OpenGameLogCommand)
        ])
    ];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(LoggingPageViewModel));
    }

    [RelayCommand]
    private async Task OpenGameLogAsync()
    {
        var logPath = GameConfig.LatestLogPath;
        if (File.Exists(logPath))
        {
            await Launcher.RevealFileAsync(logPath).ConfigureAwait(false);
            return;
        }

        var folderPath = Directory.Exists(GameConfig.MelonLoaderFolder)
            ? GameConfig.MelonLoaderFolder
            : GameConfig.Folder;

        await Launcher.OpenFolderAsync(folderPath).ConfigureAwait(false);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<LoggingPageViewModel> Logger { get; init; }

    #endregion Injections
}
