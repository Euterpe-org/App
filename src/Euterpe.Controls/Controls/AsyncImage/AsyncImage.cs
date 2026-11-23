using Avalonia.Controls.Metadata;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Euterpe.Controls;

[TemplatePart("PART_Image", typeof(Image))]
[TemplatePart("PART_PlaceholderImage", typeof(Image))]
public sealed partial class AsyncImage : TemplatedControl
{
    private static readonly HttpClient SharedHttpClient = new();

    private Bitmap? _currentBitmap;

    private Image? _imagePart;
    private CancellationTokenSource? _loadCts;
    private Image? _placeholderPart;
    private bool _reloadOnAttach;

    public static IRemoteImageLoader? DefaultRemoteLoader { get; set; }

    [GeneratedStyledProperty]
    public partial string? Source { get; set; }

    [GeneratedStyledProperty]
    public partial IImage? PlaceholderSource { get; set; }

    [GeneratedStyledProperty(DefaultValue = Stretch.Uniform)]
    public partial Stretch Stretch { get; set; }

    [GeneratedStyledProperty(DefaultValue = double.NaN)]
    public partial double DecodeWidth { get; set; }

    [GeneratedStyledProperty(DefaultValue = double.NaN)]
    public partial double DecodeHeight { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _imagePart = e.NameScope.Get<Image>("PART_Image");
        _placeholderPart = e.NameScope.Get<Image>("PART_PlaceholderImage");
        _ = LoadAsync(Source);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty && _imagePart is not null)
        {
            _ = LoadAsync(Source);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_reloadOnAttach || _imagePart is null)
        {
            return;
        }

        _reloadOnAttach = false;
        _ = LoadAsync(Source);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _loadCts?.Cancel();
        if (_imagePart is not null)
        {
            _imagePart.Source = null;
            _imagePart.Opacity = 0;
        }

        _currentBitmap?.Dispose();
        _currentBitmap = null;
        _reloadOnAttach = true;
    }

    private async Task LoadAsync(string? source)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        if (_imagePart is not null)
        {
            _imagePart.Opacity = 0;
            _imagePart.Source = null;
        }

        _placeholderPart?.IsVisible = true;

        if (string.IsNullOrEmpty(source) || !Uri.TryCreate(source, UriKind.Absolute, out var uri))
        {
            return;
        }

        var bitmap = await DecodeAsync(uri, token).ConfigureAwait(false);
        if (bitmap is null)
        {
            return;
        }

        if (token.IsCancellationRequested)
        {
            bitmap.Dispose();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested || _imagePart is null)
            {
                bitmap.Dispose();
                return;
            }

            var previous = _currentBitmap;
            _currentBitmap = bitmap;
            _imagePart.Source = bitmap;
            _imagePart.Opacity = 1;
            _placeholderPart?.IsVisible = false;

            previous?.Dispose();
        });
    }

    private async Task<Bitmap?> DecodeAsync(Uri uri, CancellationToken token)
    {
        var decodeWidth = DecodeWidth;
        var decodeHeight = DecodeHeight;
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;

        try
        {
            if (!uri.IsFile && DefaultRemoteLoader is { } loader)
            {
                var stream = await loader.OpenReadAsync(uri, token).ConfigureAwait(false);
                if (stream is null)
                {
                    return null;
                }

                await using (stream.ConfigureAwait(false))
                {
                    return await Task.Run(() => Decode(stream, decodeWidth, decodeHeight, scaling), token).ConfigureAwait(false);
                }
            }

            if (uri.IsFile)
            {
                var localPath = uri.LocalPath;
                return await Task.Run(() =>
                {
                    using var stream = File.OpenRead(localPath);
                    return Decode(stream, decodeWidth, decodeHeight, scaling);
                }, token).ConfigureAwait(false);
            }

            var httpStream = await SharedHttpClient.GetStreamAsync(uri, token).ConfigureAwait(false);
            await using (httpStream.ConfigureAwait(false))
            {
                using var memoryStream = new MemoryStream();
                await httpStream.CopyToAsync(memoryStream, token).ConfigureAwait(false);
                memoryStream.Position = 0;
                return Decode(memoryStream, decodeWidth, decodeHeight, scaling);
            }
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap Decode(Stream stream, double width, double height, double scaling)
    {
        if (!double.IsNaN(height) && height > 0)
        {
            return Bitmap.DecodeToHeight(stream, (int)Math.Ceiling(height * scaling));
        }

        if (!double.IsNaN(width) && width > 0)
        {
            return Bitmap.DecodeToWidth(stream, (int)Math.Ceiling(width * scaling));
        }

        return new Bitmap(stream);
    }
}
