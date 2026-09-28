using Avalonia.Platform.Storage;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Features.Charting;

public sealed partial class ChartManagePanelViewModel
{
    [RelayCommand]
    private async Task MigrateCustomAlbumsAsync()
    {
        var filePaths = await FileSystemPickerService.GetMultipleFilePathsAsync(FileDialog_Title_ChooseMdmFiles,
            [new FilePickerFileType(FilePickerFileType_MdmFiles) { Patterns = [$"*{CustomAlbumFiles.PackageExtension}"] }]).ConfigureAwait(true);

        if (filePaths is [])
        {
            return;
        }

        var processedCount = await ProgressDialogService.ExecuteAsync(
            XAML.ChartManage_Migrating,
            XAML.ChartManage_MigratingHint,
            false,
            progress => ChartManageService.MigrateCustomAlbumFilesAsync(filePaths, progress)).ConfigureAwait(true);
        if (processedCount is 0)
        {
            NotificationService.NoticeLight(Notification_Content_Migration_None);
        }
    }

    [RelayCommand]
    private async Task ImportChartsAsync(IReadOnlyList<IStorageItem> files)
    {
        var paths = files.GetLocalPaths().ToArray();
        if (paths is [])
        {
            return;
        }

        if (await ChartManageService.ImportChartsAsync(paths).ConfigureAwait(true))
        {
            Filter.Source = ChartSource.Offline;
        }
    }

    [RelayCommand]
    private Task ImportShareAsync() => ShareImportDialogService.ShowAsync();
}
