using System.Net;
using System.Net.Sockets;
using System.Text;
using Euterpe.Core.Http.Listeners;
using Microsoft.Extensions.Logging;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core.Http.Listeners;

[Category("LoopbackCallbackListenerTests")]
[TestSubject(typeof(LoopbackCallbackListener))]
public sealed class LoopbackCallbackListenerTest
{
    private const string DonePageUrl = "https://euterpe-org.com/auth/app/done";
    private const string State = "xyz";

    private readonly MockLogger<LoopbackCallbackListener> _logger = Mock.Logger<LoopbackCallbackListener>();

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WithCodeAndState_ReturnsCodeAndRedirectsToDonePage(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        var response = await SendAsync(listener, "GET /callback?code=abc&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(await codeTask).IsEqualTo("abc");
        await Assert.That(response).StartsWith("HTTP/1.1 302 Found");
        await Assert.That(response).Contains($"Location: {DonePageUrl}");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WithError_ReturnsNullAndBadRequest(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        var response = await SendAsync(listener, "GET /callback?error=access_denied&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(await codeTask).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 400 Bad Request");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WithCodeAndError_ReturnsNullAndBadRequest(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        var response = await SendAsync(listener, "GET /callback?code=abc&error=access_denied&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(await codeTask).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 400 Bad Request");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WithoutCodeOrError_IgnoresRequestSilentlyThenCompletesOnRealCallback(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        // A speculative / favicon request carries neither a code nor an error and must be ignored.
        var ignoredResponse = await SendAsync(listener, "GET /favicon.ico HTTP/1.1", cancellationToken);

        using (Assert.Multiple())
        {
            await Assert.That(ignoredResponse).StartsWith("HTTP/1.1 404 Not Found");
            await Assert.That(codeTask.IsCompleted).IsFalse();
            await Assert.That(_logger.Entries).IsEmpty();
        }

        await SendAsync(listener, "GET /callback?code=abc&state=xyz HTTP/1.1", cancellationToken);

        await Assert.That(await codeTask).IsEqualTo("abc");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WithMismatchedState_WarnsAndIgnoresRequestThenCompletesOnRealCallback(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        var ignoredResponse = await SendAsync(listener, "GET /callback?code=forged&state=other HTTP/1.1", cancellationToken);

        using (Assert.Multiple())
        {
            await Assert.That(ignoredResponse).StartsWith("HTTP/1.1 404 Not Found");
            await Assert.That(codeTask.IsCompleted).IsFalse();
            await Assert.That(_logger.Entries).HasSingleItem();
            await Assert.That(_logger.Entries[0].LogLevel).IsEqualTo(LogLevel.Warning);
        }

        await SendAsync(listener, "GET /callback?code=abc&state=xyz HTTP/1.1", cancellationToken);

        await Assert.That(await codeTask).IsEqualTo("abc");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WhenRequestLineSplitAcrossReads_ParsesFullLine(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.RedirectUri.Port, cancellationToken);
        await using var stream = client.GetStream();

        // Send the request line in two chunks so the listener must read past the first packet.
        await stream.WriteAsync(Encoding.ASCII.GetBytes("GET /callback?code=ab"), cancellationToken);
        await stream.FlushAsync(cancellationToken);
        await Task.Delay(50, cancellationToken);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("c&state=xyz HTTP/1.1\r\n\r\n"), cancellationToken);

        await Assert.That(await codeTask).IsEqualTo("abc");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WithLargeRequestHeaders_DeliversRedirectAfterClosing(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        var codeTask = listener.WaitForAuthorizationCodeAsync(State, cancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.RedirectUri.Port, cancellationToken);
        await using var stream = client.GetStream();
        var request = $"GET /callback?code=abc&state=xyz HTTP/1.1\r\nCookie: {new string('a', 16_000)}\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(request), cancellationToken);

        await codeTask;
        await Task.Delay(50, cancellationToken);
        var buffer = new byte[1024];
        var read = await stream.ReadAsync(buffer, cancellationToken);

        await Assert.That(Encoding.ASCII.GetString(buffer, 0, read)).StartsWith("HTTP/1.1 302 Found");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForAuthorizationCode_WhenCancelled_Throws(CancellationToken cancellationToken)
    {
        using var listener = CreateListener();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await cts.CancelAsync();

        var act = async () => await listener.WaitForAuthorizationCodeAsync(State, cts.Token);

        await Assert.That(act).Throws<OperationCanceledException>();
    }

    private LoopbackCallbackListener CreateListener() => new() { Logger = _logger };

    private static async Task<string> SendAsync(LoopbackCallbackListener listener, string requestLine, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.RedirectUri.Port, cancellationToken);
        await using var stream = client.GetStream();

        await stream.WriteAsync(Encoding.ASCII.GetBytes($"{requestLine}\r\n\r\n"), cancellationToken);

        var buffer = new byte[1024];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        return Encoding.ASCII.GetString(buffer, 0, read);
    }
}
