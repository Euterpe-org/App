using Avalonia.Platform.Storage;

namespace Euterpe.Features.Charting;

public sealed partial class ChartManagePanelViewModel
{
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
