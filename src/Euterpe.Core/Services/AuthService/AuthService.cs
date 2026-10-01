using System.Net;
using System.Security.Cryptography;
using System.Text;
using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed partial class AuthService : IAuthService
{
    private const string ClientId = "euterpe-app";
    private const string AuthorizePageUrl = "https://euterpe-org.com/auth/app";
    private static readonly TimeSpan CallbackTimeout = TimeSpan.FromMinutes(5);
    private readonly AsyncExclusiveLock _lock = new();
    public AsyncManualResetEvent Ready { get; } = new(false);

    public async Task LoginAsync()
    {
        var verifier = RandomNumberGenerator.GetBytes(32).ToBase64Url();
        var challenge = SHA256.HashData(Encoding.ASCII.GetBytes(verifier)).ToBase64Url();
        var state = RandomNumberGenerator.GetBytes(32).ToBase64Url();

        using var listener = ListenerFactory();
        var redirectUri = $"http://127.0.0.1:{listener.Port}/callback";

        var authorizeUrl = BuildAuthorizeUrl(redirectUri, challenge, state);
        try
        {
            AuthState.AuthorizeUrl = authorizeUrl;
            try
            {
                await Launcher.OpenUriAsync(authorizeUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to open the authorization page in the browser");
            }

            await CompleteBrowserLoginAsync(listener, state, verifier, redirectUri).ConfigureAwait(false);
        }
        finally
        {
            AuthState.AuthorizeUrl = null;
        }
    }

    public async Task LogoutAsync()
    {
        await _lock.StealAsync("logout").ConfigureAwait(false);
        try
        {
            if (AuthState.RefreshToken is not null)
            {
                try
                {
                    await AuthClient.LogoutAsync(new LogoutRequest(AuthState.RefreshToken)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to call logout API");
                }
            }

            await ClearSessionAsync().ConfigureAwait(false);
            Logger.LogInformation("User logged out");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        await Ready.WaitAsync().ConfigureAwait(false);

        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            if (DateTimeOffset.Now < AuthState.AccessTokenExpiry)
            {
                return AuthState.AccessToken!;
            }

            var response = await AuthClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken!)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, AuthState.CurrentUser).ConfigureAwait(false);
            return AuthState.AccessToken!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> RenewAccessTokenAsync(string staleToken)
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            if (AuthState.AccessToken != staleToken)
            {
                return AuthState.AccessToken!;
            }

            var response = await AuthClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken!)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, AuthState.CurrentUser).ConfigureAwait(false);
            return AuthState.AccessToken!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RestoreSessionAsync()
    {
        var tokens = await SecureStorage.LoadTokensAsync().ConfigureAwait(false);
        if (tokens is null)
        {
            return false;
        }

        AuthState.AccessToken = tokens.AccessToken;
        AuthState.RefreshToken = tokens.RefreshToken;

        Ready.Set();

        try
        {
            var response = await AccountClient.GetCurrentUserAsync().ConfigureAwait(false);
            AuthState.CurrentUser = response.User;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to restore session");
            await ClearSessionAsync().ConfigureAwait(false);
            return false;
        }

        return true;
    }

    public async Task<bool> IsServerHealthyAsync()
    {
        try
        {
            using var response = await HealthClient.CheckAsync().ConfigureAwait(false);
            return response.StatusCode is HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Server health check failed");
            return false;
        }
    }

    #region Injections

    public required AuthState AuthState { get; init; }
    public required Func<ILoopbackCallbackListener> ListenerFactory { get; init; }
    public required IPlatformLauncher Launcher { get; init; }
    public required IPlatformSecureStorage SecureStorage { get; init; }
    public required IEuterpeAccountClient AccountClient { get; init; }
    public required IEuterpeAuthClient AuthClient { get; init; }
    public required IEuterpeHealthClient HealthClient { get; init; }
    public required ILogger<AuthService> Logger { get; init; }

    #endregion Injections
}
