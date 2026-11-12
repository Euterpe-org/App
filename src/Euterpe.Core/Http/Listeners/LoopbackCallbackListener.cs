using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace Euterpe.Core.Http.Listeners;

internal sealed class LoopbackCallbackListener : ILoopbackCallbackListener
{
    private const string DonePageUrl = $"{EuterpeWeb.BaseUrl}/auth/app/done";

    private static readonly byte[] FoundResponse = Encoding.ASCII.GetBytes($"HTTP/1.1 302 Found\r\nLocation: {DonePageUrl}\r\nConnection: close\r\n\r\n");
    private static readonly byte[] BadRequestResponse = Encoding.ASCII.GetBytes("HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n");
    private static readonly byte[] NotFoundResponse = Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\nConnection: close\r\n\r\n");

    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);

    public LoopbackCallbackListener()
    {
        _listener.Start();
        RedirectUri = new Uri($"{Uri.UriSchemeHttp}://{_listener.LocalEndpoint}/callback");
    }

    public Uri RedirectUri { get; }

    public async Task<string?> WaitForAuthorizationCodeAsync(string state, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            using var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            var stream = client.GetStream();
            var query = ParseQuery(await ReadRequestLineAsync(stream, cancellationToken).ConfigureAwait(false));

            if (query["code"].IsNullOrEmpty() && query["error"].IsNullOrEmpty())
            {
                await stream.WriteAsync(NotFoundResponse, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (query["state"] != state)
            {
                Logger.LogWarning("Ignored a login callback with a mismatched state");
                await stream.WriteAsync(NotFoundResponse, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (query["error"] is { Length: > 0 } error)
            {
                Logger.LogWarning("Login failed with error: {Error}", error);
                await stream.WriteAsync(BadRequestResponse, cancellationToken).ConfigureAwait(false);
                return null;
            }

            await stream.WriteAsync(FoundResponse, cancellationToken).ConfigureAwait(false);
            return query["code"];
        }
    }

    public void Dispose() => _listener.Dispose();

    private static async Task<string> ReadRequestLineAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var requestLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

        string? headerLine;
        do
        {
            headerLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        } while (!headerLine.IsNullOrEmpty());

        return requestLine ?? "";
    }

    // requestLine: "GET /callback?code=...&state=... HTTP/1.1"
    private static NameValueCollection ParseQuery(string requestLine)
    {
        var target = requestLine.Split(' ').ElementAtOrDefault(1) ?? "";
        var query = target.Split('?', 2).ElementAtOrDefault(1) ?? "";
        return HttpUtility.ParseQueryString(query);
    }

    #region Injections

    public required ILogger<LoopbackCallbackListener> Logger { get; init; }

    #endregion Injections
}
