using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Desktop.Services;

public sealed partial class UpdateServiceTest
{
    [Test]
    public async Task CheckForUpdatesAsync_NotVelopackInstalled_ReturnsNullWithoutRequest()
    {
        var downloader = new TestFeedDownloader(CreateFeed("2.0.0"));
        var updateService = CreateUpdateService(feedDownloader: downloader, isInstalled: false);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsNull();
        await Assert.That(downloader.LastUrl).IsNull();
        _logger.VerifyLog().ContainingMessage(
            "Skipping update check because the application is not running from a Velopack installation");
    }

    [Test]
    [Arguments(UpdateChannel.Stable, LowerStableVersion, CurrentStableVersion, TestRuntimeIdentifier + "-stable")]
    [Arguments(UpdateChannel.Beta, LowerBetaVersion, CurrentBetaVersion, TestRuntimeIdentifier + "-beta")]
    [Arguments(UpdateChannel.Stable, CurrentStableVersion, CurrentStableVersion, TestRuntimeIdentifier + "-stable")]
    [Arguments(UpdateChannel.Beta, CurrentBetaVersion, CurrentBetaVersion, TestRuntimeIdentifier + "-beta")]
    public async Task CheckForUpdatesAsync_NoNewerVersion_ReturnsNull(
        UpdateChannel channel,
        string remoteVersion,
        string currentVersion,
        string expectedChannel)
    {
        Config.UpdateChannel = channel;
        var downloader = new TestFeedDownloader(CreateFeed(remoteVersion));

        var updateService = CreateUpdateService(currentVersion: currentVersion, feedDownloader: downloader);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsNull();
        await Assert.That(downloader.LastUrl).StartsWith(
            $"{EuterpeApi.Distribution.VelopackUrl}/{TestRuntimeIdentifier}/releases.{expectedChannel}.json?");
        _logger.VerifyLog()
            .ContainingMessage($"Checking for updates on channel {expectedChannel}")
            .ContainingMessage("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_EmptyFeed_ReturnsNull()
    {
        var updateService = CreateUpdateService();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsNull();
        _logger.VerifyLog().ContainingMessage("No new version available");
    }

    [Test]
    [Arguments(UpdateChannel.Stable, "2.0.0", CurrentStableVersion, TestRuntimeIdentifier + "-stable")]
    [Arguments(UpdateChannel.Beta, "2.0.0-beta.1", CurrentBetaVersion, TestRuntimeIdentifier + "-beta")]
    public async Task CheckForUpdatesAsync_NewerVersion_ReturnsVersion(
        UpdateChannel channel,
        string remoteVersion,
        string currentVersion,
        string expectedChannel)
    {
        Config.UpdateChannel = channel;
        var downloader = new TestFeedDownloader(CreateFeed(remoteVersion));

        var updateService = CreateUpdateService(currentVersion: currentVersion, feedDownloader: downloader);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsEqualTo(remoteVersion);
        await Assert.That(downloader.LastUrl).StartsWith(
            $"{EuterpeApi.Distribution.VelopackUrl}/{TestRuntimeIdentifier}/releases.{expectedChannel}.json?");
        _logger.VerifyLog()
            .ContainingMessage($"Checking for updates on channel {expectedChannel}")
            .ContainingMessage($"New version available: {remoteVersion}");
    }
}
