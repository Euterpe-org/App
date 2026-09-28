using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Euterpe.Services;

internal sealed partial class UpdateService
{
    internal IVelopackLocator? VelopackLocatorOverride { get; init; }

    private string GetVelopackChannel(string runtimeIdentifier)
    {
        var channelSuffix = Config.UpdateChannel switch
        {
            UpdateChannel.Stable => "stable",
            UpdateChannel.Beta => "beta",
            _ => throw new UnreachableException()
        };
        return $"{runtimeIdentifier}-{channelSuffix}";
    }

    private UpdateManager CreateUpdateManager(string runtimeIdentifier, string channel)
    {
        var feedBaseUrl = $"{EuterpeApi.Distribution.VelopackUrl}/{runtimeIdentifier}/";
        var source = new SimpleWebSource(feedBaseUrl, FeedDownloader);
        return new UpdateManager(source, new UpdateOptions { ExplicitChannel = channel }, VelopackLocatorOverride);
    }
}
