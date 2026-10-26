using Avalonia.Controls.Metadata;
using Avalonia.Labs.AnimatedImage;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace Euterpe.Controls;

[TemplatePart("PART_Animated", typeof(AnimatedImage))]
public sealed class CoverImage : TemplatedControl
{
    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<CoverImage, string?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<CoverImage, Stretch>(nameof(Stretch), Stretch.Uniform);

    private AnimatedImage? _animatedPart;
    private CancellationTokenSource? _loadCts;
    private bool _reloadOnAttach;

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _animatedPart = e.NameScope.Get<AnimatedImage>("PART_Animated");
        _ = LoadAsync(Source);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty && _animatedPart is not null)
        {
            _ = LoadAsync(Source);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_reloadOnAttach || _animatedPart is null)
        {
            return;
        }

        _reloadOnAttach = false;
        _ = LoadAsync(Source);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CancelLoad();
        _ = SetSourceAsync(null);
        _reloadOnAttach = true;
    }

    private async Task LoadAsync(string? source)
    {
        CancelLoad();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;
        var bytes = await ReadSourceAsync(source, token).ConfigureAwait(false);
        var bitmap = bytes is null ? null : await DecodeAsync(bytes, token).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested)
            {
                bitmap?.Dispose();
                return;
            }

            _ = SetSourceAsync(bitmap);
        });
    }

    private static async Task<IAnimatedBitmap?> DecodeAsync(byte[] bytes, CancellationToken token)
    {
        var bitmap = IAnimatedBitmap.Load(new MemoryStream(bytes, false), true);
        await using (token.Register(bitmap.Dispose).ConfigureAwait(false))
        {
            await Task.Run(bitmap.Init).ConfigureAwait(false);
        }

        if (bitmap.IsInitialized)
        {
            return bitmap;
        }

        bitmap.Dispose();
        return null;
    }

    private void CancelLoad()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    private static async Task<byte[]?> ReadSourceAsync(string? source, CancellationToken token)
    {
        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri))
        {
            return null;
        }

        try
        {
            if (uri.IsFile)
            {
                return await File.ReadAllBytesAsync(uri.LocalPath, token).ConfigureAwait(false);
            }

            var loader = AsyncImage.DefaultRemoteLoader;
            if (loader is null)
            {
                return null;
            }

            var stream = await loader.OpenReadAsync(uri, token).ConfigureAwait(false);
            if (stream is null)
            {
                return null;
            }

            await using (stream.ConfigureAwait(false))
            {
                using var buffer = new MemoryStream();
                await stream.CopyToAsync(buffer, token).ConfigureAwait(false);
                return buffer.ToArray();
            }
        }
        catch
        {
            return null;
        }
    }

    private async Task SetSourceAsync(IAnimatedBitmap? source)
    {
        if (_animatedPart is null)
        {
            return;
        }

        var previous = _animatedPart.Source;
        _animatedPart.Source = source;
        if (previous is null)
        {
            return;
        }

        if (ElementComposition.GetElementVisual(_animatedPart)?.Compositor is { } compositor)
        {
            await compositor.RequestCommitAsync().ConfigureAwait(false);
        }

        previous.Dispose();
    }
}
