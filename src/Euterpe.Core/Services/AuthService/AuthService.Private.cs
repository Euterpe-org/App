using System.Web;
using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed partial class AuthService
{
    private static string BuildAuthorizeUrl(string redirectUri, string codeChallenge, string state)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["client_id"] = ClientId;
        query["redirect_uri"] = redirectUri;
        query["code_challenge"] = codeChallenge;
        query["code_challenge_method"] = "S256";
        query["state"] = state;

        return $"{AuthorizePageUrl}?{query}";
    }

    private async Task CompleteBrowserLoginAsync(ILoopbackCallbackListener listener, string state, string verifier, string redirectUri)
    {
        using var cts = new CancellationTokenSource(CallbackTimeout);
        string? code;
        try
        {
            code = await listener.WaitForAuthorizationCodeAsync(state, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Login timed out waiting for the authorization callback");
            return;
        }
        catch (IOException ex)
        {
            Logger.LogWarning(ex, "Login callback connection failed");
            return;
        }

        if (code is null)
        {
            return;
        }

        try
        {
            await ExchangeCodeAsync(code, verifier, redirectUri).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Login failed during token exchange");
        }
    }

    private async Task ExchangeCodeAsync(string code, string codeVerifier, string redirectUri)
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            var response = await AuthClient.ExchangeAppTokenAsync(new AppTokenRequest(ClientId, code, codeVerifier, redirectUri)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, response.Me).ConfigureAwait(false);

            Logger.LogInformation("User logged in: {Nickname}", response.Me.Nickname);

            Ready.Set();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task UpdateSessionAsync(string accessToken, string refreshToken, UserInfo? currentUser)
    {
        AuthState.AccessToken = accessToken;
        AuthState.RefreshToken = refreshToken;
        AuthState.AccessTokenExpiry = DateTimeOffset.Now.Add(AuthConstants.AccessTokenLifetime);
        AuthState.CurrentUser = currentUser;

        await SecureStorage.SaveTokensAsync(accessToken, refreshToken).ConfigureAwait(false);
    }

    private async Task ClearSessionAsync()
    {
        AuthState.Clear();
        await SecureStorage.ClearTokensAsync().ConfigureAwait(false);
        Ready.Reset();
    }
}
