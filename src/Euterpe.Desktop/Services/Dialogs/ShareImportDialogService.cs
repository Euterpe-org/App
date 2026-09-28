namespace Euterpe.Services;

public sealed class ShareImportDialogService
{
    public async Task ShowAsync(string? shareText = null)
    {
        await ShareImportDialogViewModel.PrepareAsync(shareText).ConfigureAwait(true);

        var options = new OverlayDialogOptions
        {
            Title = XAML.Share_Import_Header,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = true
        };

        await DialogService.ShowOverlayAsync<ShareImportDialog, ShareImportDialogViewModel>(
            ShareImportDialogViewModel, options, MainWindowViewModel.DialogHostId).ConfigureAwait(true);
        ShareImportDialogViewModel.CancelImport();
    }

    #region Injections

    public required IDialogService DialogService { get; init; }
    public required ShareImportDialogViewModel ShareImportDialogViewModel { get; init; }

    #endregion Injections
}
