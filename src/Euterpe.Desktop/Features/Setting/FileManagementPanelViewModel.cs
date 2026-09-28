namespace Euterpe.Features.Setting;

[Route("/setting/file", DisplayName = Panel_Setting_FileManagement, Order = 3)]
public sealed partial class FileManagementPanelViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(FileManagementPanelViewModel));
    }

    [RelayCommand]
    private async Task ChangeGameFolderAsync() =>
        GameConfig.Folder = await GameLocalService.GetGameFolderAsync().ConfigureAwait(false);

    [RelayCommand]
    private async Task ChangeCacheFolderAsync() =>
        Config.CacheFolder = await AppLocalService.GetCacheFolderAsync().ConfigureAwait(false);

    #region Injections

    public required Config Config { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required IAppLocalService AppLocalService { get; init; }
    public required IGameLocalService GameLocalService { get; init; }
    public required ILogger<FileManagementPanelViewModel> Logger { get; init; }

    #endregion Injections
}
