namespace Euterpe.Features.Charting;

[Route("/charting/toolkit", DisplayName = Panel_Charting_CharterToolkit, Order = 1)]
public sealed partial class CharterToolkitPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ViewModelBase? ActiveTool { get; set; }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Editor.CloseRequested += OnEditorCloseRequested;
        Editor.Saved += OnEditorSaved;

        Logger.LogInformation("{ViewModel} Initialized", nameof(CharterToolkitPanelViewModel));
    }

    [RelayCommand]
    private async Task OpenEpkEditorAsync()
    {
        if (await FileSystemPickerService.GetSingleFilePathAsync(FileDialog_Title_ChooseEpkFile).ConfigureAwait(true) is { } filePath)
        {
            await OpenEpkAsync(filePath).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task NewEpkEditorAsync()
    {
        if (await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseChartFolder).ConfigureAwait(true) is not { } folder)
        {
            return;
        }

        var manifestPath = Path.Combine(folder, ChartFiles.ManifestFileName);
        if (File.Exists(manifestPath))
        {
            await OpenEpkAsync(manifestPath).ConfigureAwait(true);
            return;
        }

        Editor.CreateNew(folder);
        ActiveTool = Editor;
    }

    public async Task OpenEpkAsync(string filePath)
    {
        Manifest manifest;
        try
        {
            manifest = await MessagePackSerialization.DeserializeManifestFromFileAsync(filePath).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to read EPK file {FilePath}", filePath);
            NotificationService.ErrorLight(Notification_Content_Epk_Open_Invalid);
            return;
        }

        if (manifest.Cid is not null)
        {
            NotificationService.NoticeLight(Notification_Content_Epk_Edit_OnlineReadonly);
            return;
        }

        Editor.Open(filePath, manifest);
        ActiveTool = Editor;
    }

    private void OnEditorCloseRequested() => ActiveTool = null;

    private void OnEditorSaved(string folderPath) =>
        ChartManageService.RefreshChartAsync(folderPath).SafeFireAndForget(ex => Logger.LogError(ex, "Failed to refresh chart after editing {FolderPath}", folderPath));

    #region Injections

    public required EpkEditorPanelViewModel Editor { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required ILogger<CharterToolkitPanelViewModel> Logger { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
