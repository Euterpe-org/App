namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    private async Task HandleChartActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["convert"]:
                await MigrateAllCustomAlbumsAsync().ConfigureAwait(false);
                break;

            case ["download", var cid]:
                await DownloadChartAsync(cid).ConfigureAwait(false);
                break;

            case ["update"]:
                await ChartManageService.UpdateAllChartsAsync().ConfigureAwait(false);
                break;

            case ["update", var cid]:
                await ChartManageService.UpdateChartAsync(cid).ConfigureAwait(false);
                break;

            default:
                Logger.LogWarning("Unknown chart deep link path: {Path}", path);
                break;
        }
    }

    private async Task MigrateAllCustomAlbumsAsync()
    {
        var processedCount = await ProgressDialogService.ExecuteAsync(
            XAML.ChartManage_Migrating,
            XAML.ChartManage_MigratingHint,
            false,
            progress => ChartManageService.MigrateCustomAlbumsAsync(progress)).ConfigureAwait(true);
        if (processedCount is 0)
        {
            NotificationService.NoticeLight(Notification_Content_Migration_None);
        }
    }

    private Task DownloadChartAsync(string cid) =>
        ProgressDialogService.ExecuteAsync(
            XAML.ChartManage_Downloading,
            XAML.ChartManage_DownloadingHint,
            true,
            progress => ChartManageService.DownloadChartAsync(cid, progress));
}
