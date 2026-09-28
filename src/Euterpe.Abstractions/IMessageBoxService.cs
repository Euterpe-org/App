namespace Euterpe.Abstractions;

public interface IMessageBoxService
{
    #region Confirm

    // Normal
    Task<bool> WarningConfirmAsync(string message);
    Task<bool> WarningConfirmAsync(string message, params ReadOnlySpan<object> args);
    Task<bool> NoticeConfirmAsync(string message);
    Task<bool> NoticeConfirmAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task<bool> NoticeConfirmOverlayAsync(string message);
    Task<bool> NoticeConfirmOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion

    #region Error

    // Normal
    Task ErrorAsync(string message);
    Task ErrorAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task ErrorOverlayAsync(string message);
    Task ErrorOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion

    #region Notice

    // Normal
    Task NoticeAsync(string message);
    Task NoticeAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task NoticeOverlayAsync(string message);

    #endregion

    #region Success

    // Normal
    Task SuccessAsync(string message);
    Task SuccessAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task SuccessOverlayAsync(string message);
    Task SuccessOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion
}
