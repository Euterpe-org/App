using Avalonia.Controls.Notifications;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Services;

internal sealed class NotificationService : INotificationService, INotificationServiceWiring
{
    private WindowNotificationManager? _notifier;

    WindowNotificationManager INotificationServiceWiring.Notifier
    {
        set => _notifier = value;
    }

    private void Show(Notification notification, NotificationType type, TimeSpan? expiration, string[]? classes = null)
    {
        if (_notifier is not { } notifier)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            notifier.Show(notification, type, expiration, classes: classes);
        }
        else
        {
            Dispatcher.UIThread.Post(() => notifier.Show(notification, type, expiration, classes: classes));
        }
    }

    #region Success

    public void Success(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Success, content), NotificationType.Success, expiration);

    public void Success(string content, params ReadOnlySpan<object> args) =>
        Success(string.Format(content, args));

    public void SuccessLight(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Success, content), NotificationType.Success, expiration, ["Light"]);

    public void SuccessLight(string content, params ReadOnlySpan<object> args) =>
        SuccessLight(string.Format(content, args));

    #endregion Success

    #region Notice

    public void Notice(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Notice, content), NotificationType.Information, expiration);

    public void Notice(string content, params ReadOnlySpan<object> args) =>
        Notice(string.Format(content, args));

    public void NoticeLight(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Notice, content), NotificationType.Information, expiration, ["Light"]);

    public void NoticeLight(string content, params ReadOnlySpan<object> args) =>
        NoticeLight(string.Format(content, args));

    #endregion Notice

    #region Error

    public void Error(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Error, content), NotificationType.Error, expiration);

    public void Error(string content, params ReadOnlySpan<object> args) =>
        Error(string.Format(content, args));

    public void ErrorLight(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Error, content), NotificationType.Error, expiration, ["Light"]);

    public void ErrorLight(string content, params ReadOnlySpan<object> args) =>
        ErrorLight(string.Format(content, args));

    #endregion Error

    #region Warning

    public void Warning(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Warning, content), NotificationType.Warning, expiration);

    public void Warning(string content, params ReadOnlySpan<object> args) =>
        Warning(string.Format(content, args));

    public void WarningLight(string content, TimeSpan? expiration = null) =>
        Show(new Notification(Title_Warning, content), NotificationType.Warning, expiration, ["Light"]);

    public void WarningLight(string content, params ReadOnlySpan<object> args) =>
        WarningLight(string.Format(content, args));

    #endregion Warning
}
