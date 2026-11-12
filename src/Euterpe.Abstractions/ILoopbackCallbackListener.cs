namespace Euterpe.Abstractions;

public interface ILoopbackCallbackListener : IDisposable
{
    /// <summary>
    ///     The OAuth redirect URI on 127.0.0.1 this listener receives the authorization response on, with a port assigned by the OS.
    /// </summary>
    Uri RedirectUri { get; }

    /// <summary>
    ///     Wait for the authorization response carrying <paramref name="state" />; every other request is answered with 404 and ignored.
    /// </summary>
    /// <returns>The authorization code, or null when the authorization was denied.</returns>
    Task<string?> WaitForAuthorizationCodeAsync(string state, CancellationToken cancellationToken = default);
}
