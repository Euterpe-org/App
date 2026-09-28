using Euterpe.Models.Progress;

namespace Euterpe.Services;

public sealed class ProgressDialogService
{
    public Task ExecuteAsync(
        string title,
        string hint,
        bool isIndeterminate,
        Func<IProgress<BatchProgress>, Task> operation) =>
        ExecuteCoreAsync(title, hint, isIndeterminate, async progress =>
        {
            await operation(progress).ConfigureAwait(true);
            return true;
        });

    public Task<TResult> ExecuteAsync<TResult>(
        string title,
        string hint,
        bool isIndeterminate,
        Func<IProgress<BatchProgress>, Task<TResult>> operation) =>
        ExecuteCoreAsync(title, hint, isIndeterminate, operation);

    private async Task<TResult> ExecuteCoreAsync<TResult>(
        string title,
        string hint,
        bool isIndeterminate,
        Func<IProgress<BatchProgress>, Task<TResult>> operation)
    {
        ProgressDialogViewModel.Reset();
        ProgressDialogViewModel.IsIndeterminate = isIndeterminate;
        ProgressDialogViewModel.Hint = hint;

        var options = new OverlayDialogOptions
        {
            Title = title,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        var dialog = DialogService.ShowOverlayAsync<ProgressDialog, ProgressDialogViewModel>(
            ProgressDialogViewModel, options, MainWindowViewModel.DialogHostId);
        try
        {
            var progress = new Progress<BatchProgress>(ProgressDialogViewModel.Report);
            return await operation(progress).ConfigureAwait(true);
        }
        finally
        {
            ProgressDialogViewModel.Close();
            await dialog.ConfigureAwait(true);
        }
    }

    #region Injections

    public required IDialogService DialogService { get; init; }
    public required ProgressDialogViewModel ProgressDialogViewModel { get; init; }

    #endregion Injections
}
