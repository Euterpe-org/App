namespace Euterpe.Features.Modding;

[Route("/modding/develop", DisplayName = Panel_Modding_ModDevelop, Order = 2)]
public sealed partial class ModDevelopPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleModTemplateInstallCommand))]
    public partial bool DotNetSdkInstalled { get; set; }

    [ObservableProperty]
    public partial bool ModTemplateInstalled { get; set; }

    [ObservableProperty]
    public partial bool EnvVariableSet { get; set; }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        DotNetSdkInstalled = await SdkInstaller.CheckInstalledAsync().ConfigureAwait(true);
        ModTemplateInstalled = await ModTemplateInstaller.CheckInstalledAsync().ConfigureAwait(true);
        EnvVariableSet = PathEnvironment.IsSet();

        Logger.LogInformation("{ViewModel} Initialized", nameof(ModDevelopPanelViewModel));
    }

    [RelayCommand]
    private async Task InstallDotNetSdkAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_DotNetSDK_Install).ConfigureAwait(true);
        if (!result)
        {
            return;
        }

        Logger.LogInformation("Installing DotNet SDK");
        try
        {
            await SdkInstaller.InstallAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install DotNet SDK");
            await MessageBoxService.ErrorAsync(MessageBox_Content_DotNetSDK_Install_Failed).ConfigureAwait(true);
            return;
        }

        Logger.LogInformation("DotNet SDK installed successfully");
        DotNetSdkInstalled = true;
        await MessageBoxService.SuccessAsync(MessageBox_Content_DotNetSDK_Install_Success).ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(DotNetSdkInstalled))]
    private Task ToggleModTemplateInstallAsync() =>
        !ModTemplateInstalled ? InstallModTemplateAsync() : UninstallModTemplateAsync();

    private async Task InstallModTemplateAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_ModTemplate_Install).ConfigureAwait(true);
        if (!result)
        {
            return;
        }

        Logger.LogInformation("Installing Mod Template");
        try
        {
            await ModTemplateInstaller.InstallAsync().ConfigureAwait(true);
            Logger.LogInformation("Mod Template installed successfully");
            ModTemplateInstalled = true;
            await MessageBoxService.SuccessAsync(MessageBox_Content_ModTemplate_Install_Success).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install Mod Template");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ModTemplate_Install_Failed).ConfigureAwait(false);
        }
    }

    private async Task UninstallModTemplateAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_ModTemplate_Uninstall).ConfigureAwait(true);
        if (!result)
        {
            return;
        }

        Logger.LogInformation("Uninstalling Mod Template");
        try
        {
            await ModTemplateInstaller.UninstallAsync().ConfigureAwait(true);
            Logger.LogInformation("Mod Template uninstalled successfully");
            ModTemplateInstalled = false;
            await MessageBoxService.SuccessAsync(MessageBox_Content_ModTemplate_Uninstall_Success).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to uninstall Mod Template");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ModTemplate_Uninstall_Failed).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task SetEnvVariableAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_SetMdDirectoryEnvironment)
            .ConfigureAwait(true);
        if (!result)
        {
            return;
        }

        Logger.LogInformation("Setting {VariableName} environment variable", GameConfig.PathEnvironmentVariableName);

        var success = PathEnvironment.Set();
        if (success)
        {
            Logger.LogInformation("{VariableName} environment variable set successfully", GameConfig.PathEnvironmentVariableName);
            EnvVariableSet = true;
        }
        else
        {
            Logger.LogError("Failed to set {VariableName} environment variable", GameConfig.PathEnvironmentVariableName);
            await MessageBoxService.ErrorAsync(MessageBox_Content_SetMdDirectoryEnvironment_Failed).ConfigureAwait(false);
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IDotNetSdkInstaller SdkInstaller { get; init; }
    public required IGameModTemplateInstaller ModTemplateInstaller { get; init; }
    public required IGamePathEnvironment PathEnvironment { get; init; }
    public required ILogger<ModDevelopPanelViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
