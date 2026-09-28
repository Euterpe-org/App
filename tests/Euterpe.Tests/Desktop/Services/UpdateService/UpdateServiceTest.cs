using TUnit.Mocks.Logging;
using Velopack;
using Velopack.Locators;
using Velopack.Logging;
using Velopack.Sources;

namespace Euterpe.Tests.Desktop.Services;

[Category("UpdateServiceTests")]
[TestSubject(typeof(UpdateService))]
public sealed partial class UpdateServiceTest
{
    private const string AppId = "Euterpe";
    private const string CurrentStableVersion = "1.0.0";
    private const string CurrentBetaVersion = "1.0.0-beta.1";
    private const string LowerStableVersion = "0.0.1";
    private const string LowerBetaVersion = "0.0.1-beta.1";
    private const string TestRuntimeIdentifier = "test-rid";

    private readonly MockLogger<UpdateService> _logger = Mock.Logger<UpdateService>();

    private Config Config { get; } = new() { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() };

    private UpdateService CreateUpdateService(
        Config? config = null,
        string currentVersion = CurrentStableVersion,
        IFileDownloader? feedDownloader = null,
        IPlatformInfo? platformInfo = null,
        bool isInstalled = true) =>
        new()
        {
            Config = config ?? Config,
            FeedDownloader = feedDownloader ?? new TestFeedDownloader(CreateFeed()),
            Logger = _logger,
            PlatformInfo = platformInfo ?? CreatePlatformInfoMock(),
            VelopackLocatorOverride = isInstalled
                ? new TestVelopackLocator(AppId, currentVersion, AppContext.BaseDirectory)
                : CreateNotInstalledVelopackLocator()
        };

    private static IVelopackLocator CreateNotInstalledVelopackLocator()
    {
        var locator = IVelopackLocator.Mock();
        locator.CurrentlyInstalledVersion.Returns((SemanticVersion?)null);
        locator.Log.Returns(new NullVelopackLogger());
        return locator;
    }

    private static IPlatformInfo CreatePlatformInfoMock()
    {
        var mock = IPlatformInfo.Mock();
        mock.RuntimeIdentifier.Returns(TestRuntimeIdentifier);
        return mock;
    }

    private static string CreateFeed(string? version = null) =>
        version is null
            ? """{"Assets":[]}"""
            : $$"""{"Assets":[{"PackageId":"{{AppId}}","Version":"{{version}}","Type":"Full","FileName":"{{AppId}}-{{version}}-full.nupkg","SHA1":"0123456789012345678901234567890123456789","Size":1}]}""";

    private sealed class TestFeedDownloader(string response) : IFileDownloader
    {
        public string? LastUrl { get; private set; }

        public Task DownloadFile(
            string url,
            string targetFile,
            Action<int> progress,
            IDictionary<string, string>? headers = null,
            double timeout = 30,
            CancellationToken cancelToken = default) =>
            throw new NotSupportedException();

        public Task<byte[]> DownloadBytes(string url, IDictionary<string, string>? headers = null, double timeout = 30) => throw new NotSupportedException();

        public Task<string> DownloadString(string url, IDictionary<string, string>? headers = null, double timeout = 30)
        {
            LastUrl = url;
            return Task.FromResult(response);
        }
    }
}
