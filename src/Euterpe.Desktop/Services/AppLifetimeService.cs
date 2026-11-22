namespace Euterpe.Services;

internal sealed class AppLifetimeService : IAppLifetimeService
{
    public void Shutdown() => GetCurrentDesktop().Shutdown();
}
