namespace Euterpe.Services;

internal sealed partial class MessageBoxService : IMessageBoxService
{
    #region Confirm

    public async Task<bool> WarningConfirmAsync(string message) =>
        await ShowAsync(message, Title_Warning, MessageBoxIcon.Warning, MessageBoxButton.YesNo).ConfigureAwait(false) == MessageBoxResult.Yes;

    public Task<bool> WarningConfirmAsync(string message, params ReadOnlySpan<object> args) =>
        WarningConfirmAsync(string.Format(message, args));

    public async Task<bool> NoticeConfirmAsync(string message) =>
        await ShowAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.YesNo).ConfigureAwait(false) == MessageBoxResult.Yes;

    public Task<bool> NoticeConfirmAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeConfirmAsync(string.Format(message, args));

    public async Task<bool> NoticeConfirmOverlayAsync(string message) =>
        await ShowOverlayAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.YesNo).ConfigureAwait(false) == MessageBoxResult.Yes;

    public Task<bool> NoticeConfirmOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeConfirmOverlayAsync(string.Format(message, args));

    #endregion

    #region Error

    public Task ErrorAsync(string message) =>
        ShowAsync(message, Title_Error, MessageBoxIcon.Error, MessageBoxButton.OK);

    public Task ErrorAsync(string message, params ReadOnlySpan<object> args) =>
        ErrorAsync(string.Format(message, args));

    public Task ErrorOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Error, MessageBoxIcon.Error, MessageBoxButton.OK);

    public Task ErrorOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        ErrorOverlayAsync(string.Format(message, args));

    #endregion

    #region Notice

    public Task NoticeAsync(string message) =>
        ShowAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.OK);

    public Task NoticeAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeAsync(string.Format(message, args));

    public Task NoticeOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.OK);

    #endregion

    #region Success

    public Task SuccessAsync(string message) =>
        ShowAsync(message, Title_Success, MessageBoxIcon.Success, MessageBoxButton.OK);

    public Task SuccessAsync(string message, params ReadOnlySpan<object> args) =>
        SuccessAsync(string.Format(message, args));

    public Task SuccessOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Success, MessageBoxIcon.Success, MessageBoxButton.OK);

    public Task SuccessOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        SuccessOverlayAsync(string.Format(message, args));

    #endregion
}
