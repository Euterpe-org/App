using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Services;

public interface IDialogService
{
    Task<T?> ShowWindowDialogAsync<TWindow, TViewModel, T>(TViewModel vm, Window? owner = null)
        where TWindow : Window, new()
        where TViewModel : class, IDialog<T>;

    Task<T?> ShowOverlayAsync<TView, TViewModel, T>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext;

    Task ShowOverlayAsync<TView, TViewModel>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext;

    Task<DialogResult> ShowStandardOverlayAsync<TView, TViewModel>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext;
}
