namespace Euterpe.Services;

public sealed class UpdateDialogService
{
    public async Task<bool> ShowAsync(string version, string hostId)
    {
        var vm = UpdateDialogViewModelFactory(version);

        var options = new OverlayDialogOptions
        {
            CanDragMove = false,
            CanLightDismiss = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        var dialogCompletion = DialogService.ShowOverlayAsync<UpdateDialog, UpdateDialogViewModel>(vm, options, hostId);
        var succeeded = true;

        try
        {
            await UpdateService.UpdateAsync(new Progress<int>(vm.Report)).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            succeeded = false;
            Logger.LogError(ex, "Failed to update to version {Version}", version);
        }
        finally
        {
            vm.Close();
            await dialogCompletion.ConfigureAwait(true);
        }

        if (!succeeded)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_Update_Failed, version).ConfigureAwait(false);
        }

        return succeeded;
    }

    #region Injections

    public required IDialogService DialogService { get; init; }
    public required ILogger<UpdateDialogService> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IUpdateService UpdateService { get; init; }
    public required Func<string, UpdateDialogViewModel> UpdateDialogViewModelFactory { get; init; }

    #endregion Injections
}
