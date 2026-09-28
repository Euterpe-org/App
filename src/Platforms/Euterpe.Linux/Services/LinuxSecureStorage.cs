using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Euterpe.Contracts.Account;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxSecureStorage : IPlatformSecureStorage
{
    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        var payload = new TokenPayload(accessToken, refreshToken);
        var json = JsonSerializer.Serialize(payload, Default.TokenPayload);

        try
        {
            var result = await Cli.Wrap("secret-tool")
                .WithArguments(["store", "--label", AppName, "app", AppName])
                .WithStandardInputPipe(PipeSource.FromString(json))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(true);

            if (result.ExitCode is 0)
            {
                return;
            }

            Logger.LogWarning("secret-tool store failed with exit code {ExitCode}", result.ExitCode);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "secret-tool is not available");
            await MessageBoxService.NoticeAsync(MessageBox_Content_SecretService_Unavailable).ConfigureAwait(false);
        }
    }

    public async Task<TokenPayload?> LoadTokensAsync()
    {
        try
        {
            var result = await Cli.Wrap("secret-tool")
                .WithArguments(["lookup", "app", AppName])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0 || result.StandardOutput.IsNullOrEmpty())
            {
                return null;
            }

            var payload = JsonSerializer.Deserialize(result.StandardOutput, Default.TokenPayload);
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
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load auth tokens");
            return null;
        }
    }

    public async Task ClearTokensAsync()
    {
        try
        {
            await Cli.Wrap("secret-tool")
                .WithArguments(["clear", "app", AppName])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to clear tokens with secret-tool");
        }
    }

    #region Injections

    public required ILogger<LinuxSecureStorage> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
