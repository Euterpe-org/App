using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Services;

internal sealed class DialogService : IDialogService
{
    public async Task<T?> ShowWindowDialogAsync<TWindow, TViewModel, T>(TViewModel vm, Window? owner = null)
        where TWindow : Window, new()
        where TViewModel : class, IDialog<T>
    {
        var window = new TWindow { DataContext = vm };

        T? result = default;
        EventHandler<T> handler = (_, value) =>
        {
            result = value;
            window.Close();
        };

        vm.RequestClose += handler;

        try
        {
            owner ??= GetCurrentDesktop().MainWindow;
            if (owner is { IsVisible: true })
            {
                window.Icon ??= owner.Icon;
                await window.ShowDialog(owner).ConfigureAwait(true);
            }
            else
            {
                var closed = new TaskCompletionSource();
                window.Closed += (_, _) => closed.TrySetResult();
                window.Show();
                await closed.Task.ConfigureAwait(true);
            }

            return result;
        }
        finally
        {
            vm.RequestClose -= handler;
        }
    }

    public Task<T?> ShowOverlayAsync<TView, TViewModel, T>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext =>
        OverlayDialog.ShowCustomAsync<TView, TViewModel, T>(vm, hostId, options, token);

    public async Task ShowOverlayAsync<TView, TViewModel>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext =>
        await OverlayDialog.ShowCustomAsync<TView, TViewModel, object>(vm, hostId, options, token).ConfigureAwait(true);

    public Task<DialogResult> ShowStandardOverlayAsync<TView, TViewModel>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext =>
        OverlayDialog.ShowStandardAsync<TView, TViewModel>(vm, hostId, options, token);
}
