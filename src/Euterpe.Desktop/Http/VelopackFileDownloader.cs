using Velopack.Sources;

namespace Euterpe.Http;

internal sealed class VelopackFileDownloader : HttpClientFileDownloader
{
    #region Injections

    public required IHttpClientFactory HttpClientFactory { get; init; }

    #endregion Injections

    protected override HttpClient CreateHttpClient(IDictionary<string, string>? headers, double timeout)
    {
        var client = HttpClientFactory.CreateClient(nameof(EuterpeApi.Distribution));
        client.Timeout = TimeSpan.FromMinutes(timeout);
        client.DefaultRequestHeaders.UserAgent.Add(UserAgent);

        if (headers is null)
        {
            return client;
        }

        foreach (var (key, value) in headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }

        return client;
    }
}
