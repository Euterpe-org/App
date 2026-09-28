namespace Euterpe.Services;

internal sealed partial class MessageBoxService
{
    private static Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxIcon icon, MessageBoxButton button) =>
        Dispatcher.UIThread.CheckAccess()
            ? MessageBox.ShowAsync(message, title, icon, button)
            : Dispatcher.UIThread.InvokeAsync(() => MessageBox.ShowAsync(message, title, icon, button));

    private static Task<MessageBoxResult> ShowOverlayAsync(string message, string title, MessageBoxIcon icon, MessageBoxButton button) =>
        Dispatcher.UIThread.CheckAccess()
            ? OverlayMessageBox.ShowAsync(message, title, icon: icon, button: button)
            : Dispatcher.UIThread.InvokeAsync(() => OverlayMessageBox.ShowAsync(message, title, icon: icon, button: button));
}
