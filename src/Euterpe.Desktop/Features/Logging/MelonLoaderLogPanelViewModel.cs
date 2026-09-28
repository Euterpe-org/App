namespace Euterpe.Features.Logging;

[Route("/logging/melonloader", DisplayName = Panel_Logging_MelonLoaderLog, Order = 1)]
public sealed partial class MelonLoaderLogPanelViewModel : ViewModelBase
{
    private string _logContent = string.Empty;

    public bool HasLogLines => LogLines.Length > 0;

    [NotifyPropertyChangedFor(nameof(HasLogLines))]
    [ObservableProperty]
    public partial string[] LogLines { get; private set; } = [];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        await RefreshLogAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(MelonLoaderLogPanelViewModel));
    }

    [RelayCommand]
    private async Task RefreshLogAsync()
    {
        var stream = FileSystemService.TryOpenSharedReadFile(GameConfig.LatestLogPath);
        if (stream is null)
        {
            SetLogContent(string.Empty);
            return;
        }

        try
        {
            await using (stream.ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(true);
                SetLogContent(content);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read MelonLoader log {FilePath}", GameConfig.LatestLogPath);
            SetLogContent(string.Empty);
        }
    }

    [RelayCommand]
    private Task AnalyzeLogAsync() => Task.CompletedTask;

    private void SetLogContent(string content)
    {
        _logContent = content;
        LogLines = _logContent.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<MelonLoaderLogPanelViewModel> Logger { get; init; }

    #endregion Injections
}
