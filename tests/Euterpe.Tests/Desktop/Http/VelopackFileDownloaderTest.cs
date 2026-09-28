using System.Net;
using Euterpe.Core.Extensions;
using Euterpe.Http;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Mocks.Http;
using Velopack.Sources;

namespace Euterpe.Tests.Desktop.Http;

[Category("VelopackFileDownloaderTests")]
[TestSubject(typeof(VelopackFileDownloader))]
public sealed class VelopackFileDownloaderTest
{
    [Test]
    public async Task DownloadString_DistributionHttpClient_SendsRequestIdAndAuthorization()
    {
        const string feedPath = "/api/distribution/velopack/test-rid/releases.test-rid.json";
        var authService = IAuthService.Mock();
        authService.GetAccessTokenAsync().Returns("token");
        var primary = Mock.HttpHandler();
        primary.OnGet(feedPath).RespondWithJson("""{"Assets":[]}""");

        await using var provider = CreateProvider(authService, primary);
        var downloader = CreateDownloader(provider);
        var feedUrl = $"{new Uri(EuterpeApi.BaseUrl).GetLeftPart(UriPartial.Authority)}{feedPath}";

        await downloader.DownloadString(feedUrl);

        using var assertions = Assert.Multiple();
        await Assert.That(primary.Requests).HasSingleItem();
        await Assert.That(primary.Requests[0].Headers["Authorization"].Single()).IsEqualTo("Bearer token");
        await Assert.That(Guid.TryParse(primary.Requests[0].Headers["X-Request-Id"].Single(), out _)).IsTrue();
    }

    [Test]
    public async Task DownloadFile_DistributionHttpClient_SendsAuthorizationAndWritesContent()
    {
        const string packagePath = "/api/distribution/velopack/test-rid/Euterpe-1.0.0-test-rid-full.nupkg";
        byte[] expectedContent = [1, 2, 3, 4];
        var authService = IAuthService.Mock();
        authService.GetAccessTokenAsync().Returns("token");
        var primary = Mock.HttpHandler();
        primary.OnGet(packagePath)
            .Respond()
            .WithFactory(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(expectedContent) });

        await using var provider = CreateProvider(authService, primary);
        var downloader = CreateDownloader(provider);
        var packageUrl = $"{new Uri(EuterpeApi.BaseUrl).GetLeftPart(UriPartial.Authority)}{packagePath}";
        var targetFile = Path.Combine(Path.GetTempPath(), $"euterpe-velopack-{Guid.NewGuid():N}.nupkg");

        try
        {
            await downloader.DownloadFile(packageUrl, targetFile, _ => { }, null, 1, CancellationToken.None);

            var actualContent = await File.ReadAllBytesAsync(targetFile);
            using var assertions = Assert.Multiple();
            await Assert.That(actualContent).IsEquivalentTo(expectedContent, EqualityComparer<byte>.Default, CollectionOrdering.Matching);
            await Assert.That(primary.Requests).IsNotEmpty();
            await Assert.That(primary.Requests.All(r => r.Headers["Authorization"].Single() == "Bearer token")).IsTrue();
            await Assert.That(primary.Requests.All(r => Guid.TryParse(r.Headers["X-Request-Id"].Single(), out _))).IsTrue();
        }
        finally
        {
            File.Delete(targetFile);
        }
    }

    private static ServiceProvider CreateProvider(IAuthService authService, MockHttpHandler primary)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.RegisterHttpClients();
        services.AddSingleton(authService);
        services.AddSingleton<INotificationService>(INotificationService.Mock());
        services.AddHttpClient(nameof(EuterpeApi.Distribution)).ConfigurePrimaryHttpMessageHandler(() => primary);
        return services.BuildServiceProvider();
    }

    private static IFileDownloader CreateDownloader(ServiceProvider provider) =>
        new VelopackFileDownloader
        {
            HttpClientFactory = provider.GetRequiredService<IHttpClientFactory>()
        };
}
