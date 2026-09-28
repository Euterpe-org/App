using Avalonia.Input.Platform;

namespace Euterpe.Features.Charting;

public sealed partial class ChartManagePanelViewModel
{
    [ObservableProperty]
    public partial bool IsSelectionMode { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(SelectedCountDisplay))]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial bool CanShareSelection { get; set; }

    public bool HasSelection => SelectedCount > 0;
    public string SelectedCountDisplay => string.Format(CultureInfo.CurrentCulture, XAML.ChartManage_SelectedCount, SelectedCount);

    private void ObserveSelection()
    {
        Filter.ObservePropertyChanged(static x => x.Source)
            .Subscribe(this, static (_, vm) => vm.IsSelectionMode = false);

        _sourceCache.Connect()
            .AutoRefresh(static item => item.IsSelected)
            .Filter(static item => item.IsSelected)
            .ToCollection()
            .Subscribe(selectedItems =>
            {
                SelectedCount = selectedItems.Count;
                CanShareSelection = selectedItems.Count is > 0 and <= GameSharePackage.MaximumChartCount;
            });
    }

    [RelayCommand]
    private async Task ShareSelectedAsync()
    {
        if (!CanShareSelection)
        {
            return;
        }

        var cids = _sourceCache.Items
            .Where(static item => item.IsSelected)
            .Select(static item => item.Chart.Manifest.Cid!.Value)
            .ToArray();

        var shareLink = GameShareService.CreateChartShareLink(cids);
        await TopLevel.Clipboard.SetTextAsync(shareLink).ConfigureAwait(true);
        NotificationService.SuccessLight(Notification_Content_Share_Copy_Success);

        IsSelectionMode = false;
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var folderPaths = _sourceCache.Items
            .Where(static item => item.IsSelected)
            .Select(static item => item.Chart.FolderPath)
            .ToArray();
        if (folderPaths is [])
        {
            return;
        }

        var result = await MessageBoxService.WarningConfirmAsync(MessageBox_Content_Chart_BulkDelete_Confirm, folderPaths.Length).ConfigureAwait(true);
        if (!result)
        {
            return;
        }

        if (Playback.PlayingKey is { } playingKey && folderPaths.Contains(playingKey))
        {
            AudioPlayerService.Stop();
        }

        await ChartManageService.DeleteChartsAsync(folderPaths).ConfigureAwait(true);

        IsSelectionMode = false;
    }

    partial void OnIsSelectionModeChanged(bool value)
    {
        if (value)
        {
            return;
        }

        foreach (var item in _sourceCache.Items)
        {
            item.IsSelected = false;
        }
    }
}
