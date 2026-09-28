using Avalonia.Platform;

namespace Euterpe.Services;

internal sealed class ResourceService : IResourceService
{
    public Stream GetAssetAsStream(string fileName) => AssetLoader.Open(new Uri($"avares://Euterpe.Desktop/Assets/{fileName}"));
}
