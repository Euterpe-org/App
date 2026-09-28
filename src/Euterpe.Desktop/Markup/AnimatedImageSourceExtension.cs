using Avalonia.Labs.AnimatedImage;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Markup;

[UsedImplicitly]
public sealed class AnimatedImageSourceExtension(string uri) : MarkupExtension
{
    public string? Uri { get; } = uri;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Uri))
        {
            throw new InvalidOperationException($"{nameof(AnimatedImageSourceExtension)}.{nameof(Uri)} must be set.");
        }

        var parsedUri = new Uri(Uri, UriKind.RelativeOrAbsolute);
        if (parsedUri.IsAbsoluteUri)
        {
            return IAnimatedBitmap.Load(AssetLoader.Open(parsedUri), true);
        }

        var baseUri = serviceProvider.GetRequiredService<IUriContext>().BaseUri;
        return IAnimatedBitmap.Load(AssetLoader.Open(parsedUri, baseUri), true);
    }
}
