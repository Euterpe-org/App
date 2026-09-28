namespace Euterpe.Features.Modding;

[Route("/modding/melonloader", DisplayName = Panel_Modding_MelonLoader, Order = 1)]
public sealed partial class MelonLoaderPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial InstallStatus MelonLoaderInstallStatus { get; set; }

    [ObservableProperty]
    public partial LocalizedString? DownloadText { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        MelonLoaderInstallStatus = GameConfig.MelonLoaderVersion is null ? InstallStatus.NotInstalled : InstallStatus.Installed;

        Logger.LogInformation("{ViewModel} Initialized", nameof(MelonLoaderPanelViewModel));
    }

    [RelayCommand]
    private async Task InstallMelonLoaderAsync()
    {
        Logger.LogInformation("Installing MelonLoader");
        try
        {
            MelonLoaderInstallStatus = InstallStatus.Downloading;

            var progress = new Progress<double>(value => DownloadProgress = value);
            await DependencyAcquireService.AcquireForMelonLoaderAsync(OnDownloadStarted, progress).ConfigureAwait(true);
            await GameLocalService.InstallMelonLoaderAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install MelonLoader");
            MelonLoaderInstallStatus = InstallStatus.NotInstalled;
            await MessageBoxService.ErrorAsync(MessageBox_Content_MelonLoader_Install_Failed).ConfigureAwait(true);
            return;
        }

        GameLocalService.ReadMelonLoaderVersion();
        MelonLoaderInstallStatus = InstallStatus.Installed;
        await MessageBoxService.SuccessAsync(MessageBox_Content_MelonLoader_Install_Success).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task UninstallMelonLoaderAsync()
    {
        Logger.LogInformation("Uninstalling MelonLoader");
        try
        {
            await GameLocalService.UninstallMelonLoaderAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to uninstall MelonLoader");
            await MessageBoxService.ErrorAsync(MessageBox_Content_MelonLoader_Uninstall_Failed).ConfigureAwait(true);
            return;
        }

        GameConfig.MelonLoaderVersion = null;
        MelonLoaderInstallStatus = InstallStatus.NotInstalled;
        await MessageBoxService.SuccessAsync(MessageBox_Content_MelonLoader_Uninstall_Success).ConfigureAwait(true);
    }

    private void OnDownloadStarted(object? sender, DownloadStartedEventArgs args)
    {
        var fileName = Path.GetFileName(args.FileName);
        var mbSize = args.TotalBytesToReceive / (1024d * 1024d);
        DownloadText = string.Format(XAML.MelonLoader_State_Downloading, fileName, $"{mbSize:F2}");
        Logger.LogInformation("Downloading {FileName}: {TotalBytes}B", fileName, args.TotalBytesToReceive);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IDependencyAcquireService DependencyAcquireService { get; init; }
    public required IGameLocalService GameLocalService { get; init; }
    public required ILogger<MelonLoaderPanelViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
