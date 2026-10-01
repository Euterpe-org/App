using System.Web;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests.Core;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task LoginAsync_InteractiveLogin_OpensAuthorizeUrlWithPkceChallengeAndState()
    {
        string? capturedUrl = null;
        var launcher = IPlatformLauncher.Mock();
        launcher.OpenUriAsync(Any<string>()).Callback(url => capturedUrl = url);

        var sut = CreateAuthService(launcher: launcher, listenerFactory: StaticListener(AuthCode, "tampered-state", null));
        await sut.LoginAsync();

        var query = HttpUtility.ParseQueryString(new Uri(capturedUrl!).Query);
        using var _ = Assert.Multiple();
        await Assert.That(query["client_id"]).IsEqualTo("euterpe-app");
        await Assert.That(query["code_challenge_method"]).IsEqualTo("S256");
        await Assert.That(query["code_challenge"]).IsNotNull();
        await Assert.That(query["state"]).IsNotNull();
        await Assert.That(query["redirect_uri"]).StartsWith("http://127.0.0.1:");
    }

    [Test]
    public async Task LoginAsync_BrowserLaunchFails_ExposesLinkAndCompletesManualLogin()
    {
        var authState = new AuthState();
        string? urlAtLaunch = null;
        var authClient = IEuterpeAuthClient.Mock();
        authClient.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(ValidAccessToken, ValidRefreshToken, TestUser));
        var launcher = IPlatformLauncher.Mock();
        launcher.OpenUriAsync(Any<string>()).Callback(url =>
        {
            urlAtLaunch = authState.AuthorizeUrl;
            throw new InvalidOperationException("No default browser");
        });
        var listener = ILoopbackCallbackListener.Mock();
        listener.WaitForCallbackAsync(Any<CancellationToken>()).Returns(() =>
            new LoopbackCallbackResult(AuthCode, HttpUtility.ParseQueryString(new Uri(authState.AuthorizeUrl!).Query)["state"], null));
        var sut = CreateAuthService(authClient, authState: authState, launcher: launcher, listenerFactory: () => listener);

        await sut.LoginAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(urlAtLaunch).IsNotNull();
        await Assert.That(authState.AuthorizeUrl).IsNull();
        await Assert.That(sut.Ready.IsSet).IsTrue();
        await Assert.That(sut.AuthState.AccessToken).IsEqualTo(ValidAccessToken);
        launcher.OpenUriAsync(urlAtLaunch!).WasCalled(Times.Once);
    }

    [Test]
    public async Task LoginAsync_WaitingForAuthorization_PublishesLinkUntilAttemptEnds()
    {
        var callback = new TaskCompletionSource<LoopbackCallbackResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var listener = ILoopbackCallbackListener.Mock();
        listener.WaitForCallbackAsync(Any<CancellationToken>()).Returns(() => callback.Task);
        var sut = CreateAuthService(listenerFactory: () => listener);

        var login = sut.LoginAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(sut.AuthState.AuthorizeUrl).StartsWith("https://euterpe-org.com/auth/app?");

        callback.SetResult(new LoopbackCallbackResult(null, "invalid-state", null));
        await login;

        await Assert.That(sut.AuthState.AuthorizeUrl).IsNull();
    }

    [Test]
    public async Task LoginAsync_WhenCallbackSucceeds_ShouldSetReadyAndUpdateState()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(ValidAccessToken, ValidRefreshToken, TestUser));
        var (launcher, listenerFactory) = StateEchoingLoopback(AuthCode, null);
        var sut = CreateAuthService(authClientMock, launcher: launcher, listenerFactory: listenerFactory);

        await sut.LoginAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsTrue();
        await Assert.That(sut.AuthState.AccessToken).IsEqualTo(ValidAccessToken);
        await Assert.That(sut.AuthState.RefreshToken).IsEqualTo(ValidRefreshToken);
        await Assert.That(sut.AuthState.CurrentUser).IsEqualTo(TestUser);
    }

    [Test]
    public async Task LoginAsync_WhenExchangeFails_ShouldNotSetReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Throws(new HttpRequestException("Network error"));
        var (launcher, listenerFactory) = StateEchoingLoopback(AuthCode, null);
        var sut = CreateAuthService(authClientMock, launcher: launcher, listenerFactory: listenerFactory);

        await sut.LoginAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task LoginAsync_WhenCallbackTimesOut_ShouldNotExchangeNorSetReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        var listener = ILoopbackCallbackListener.Mock();
        listener.WaitForCallbackAsync(Any<CancellationToken>()).Throws(new OperationCanceledException());
        var sut = CreateAuthService(authClientMock, listenerFactory: () => listener);

        await sut.LoginAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        await Assert.That(sut.AuthState.AuthorizeUrl).IsNull();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>()).WasNeverCalled();
    }

    [Test]
    public async Task LoginAsync_WhenCodeMissing_ShouldNotExchangeNorSetReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        var (launcher, listenerFactory) = StateEchoingLoopback(null, null);
        var sut = CreateAuthService(authClientMock, launcher: launcher, listenerFactory: listenerFactory);

        await sut.LoginAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>()).WasNeverCalled();
    }

    [Test]
    public async Task LoginAsync_WhenStateMismatch_ShouldNotExchangeNorSetReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        var sut = CreateAuthService(authClientMock, listenerFactory: StaticListener(AuthCode, "tampered-state", null));

        await sut.LoginAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>()).WasNeverCalled();
    }

    [Test]
    public async Task LoginAsync_WhenCallbackHasError_ShouldNotExchangeNorSetReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        var (launcher, listenerFactory) = StateEchoingLoopback(null, "access_denied");
        var sut = CreateAuthService(authClientMock, launcher: launcher, listenerFactory: listenerFactory);

        await sut.LoginAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsFalse();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>()).WasNeverCalled();
    }

    private static (IPlatformLauncher launcher, Func<ILoopbackCallbackListener> factory) StateEchoingLoopback(string? code, string? error)
    {
        string? capturedState = null;
        var launcher = IPlatformLauncher.Mock();
        launcher.OpenUriAsync(Any<string>())
            .Callback(url => capturedState = HttpUtility.ParseQueryString(new Uri(url).Query)["state"]);

        var listener = ILoopbackCallbackListener.Mock();
        listener.WaitForCallbackAsync(Any<CancellationToken>())
            .Returns(() => new LoopbackCallbackResult(code, capturedState, error));

        return (launcher, () => listener);
    }

    private static Func<ILoopbackCallbackListener> StaticListener(string? code, string? state, string? error)
    {
        var listener = ILoopbackCallbackListener.Mock();
        listener.WaitForCallbackAsync(Any<CancellationToken>())
            .Returns(new LoopbackCallbackResult(code, state, error));
        return () => listener;
    }
}
