using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Euterpe.Contracts.Account;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsSecureStorage : IPlatformSecureStorage
{
    private static readonly string TokenFilePath = Path.Combine(AppDataFolder, "auth.dat");

    #region Injections

    public required ILogger<WindowsSecureStorage> Logger { get; init; }

    #endregion Injections

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        try
        {
            var payload = new TokenPayload(accessToken, refreshToken);
            var json = JsonSerializer.Serialize(payload, Default.TokenPayload);
            var plainBytes = Encoding.UTF8.GetBytes(json);
            var encrypted = ProtectedData.Protect(plainBytes, DataProtectionScope.CurrentUser);

            await File.WriteAllBytesAsync(TokenFilePath, encrypted).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to save auth tokens");
        }
    }

    public async Task<TokenPayload?> LoadTokensAsync()
    {
        if (!File.Exists(TokenFilePath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllBytesAsync(TokenFilePath).ConfigureAwait(false);
            var plainBytes = ProtectedData.Unprotect(encrypted, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(plainBytes);
            var payload = JsonSerializer.Deserialize(json, Default.TokenPayload);

            if (payload == null)
            {
                Logger.LogWarning("Failed to deserialize auth tokens, clearing stored data");
                await ClearTokensAsync().ConfigureAwait(false);
                return null;
            }

            if (payload.AccessToken.IsNullOrEmpty() || payload.RefreshToken.IsNullOrEmpty())
            {
                Logger.LogWarning("Auth tokens are empty, clearing stored data");
                await ClearTokensAsync().ConfigureAwait(false);
                return null;
            }

            return payload;
        }
        catch (CryptographicException ex)
        {
            Logger.LogWarning(ex, "Failed to decrypt auth tokens, clearing stored data");
            await ClearTokensAsync().ConfigureAwait(false);
            return null;
        }
    }

    public async Task ClearTokensAsync()
    {
        if (!File.Exists(TokenFilePath))
        {
            return;
        }

        try
        {
            File.Delete(TokenFilePath);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to delete auth token file");
        }
    }
}
