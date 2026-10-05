using NLog;
using Velopack;
using static Euterpe.CrashHandler;
using static Euterpe.IocContainer;

namespace Euterpe;

public static class Bootstrapper
{
    public static void Run<TPlatform>(string[] args) where TPlatform : IPlatformServices
    {
        VelopackApp.Build().Run();

        using var mutex = new Mutex(true, AppName, out var createdNew);
        if (!createdNew)
        {
            if (args is [var argument, ..])
            {
                ActivationPipe.Send(argument);
            }

            return;
        }

        Directory.CreateDirectory(AppDataFolder);
        ConfigureContainer<TPlatform>();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            ActivationPipe.StopListening();
            LogManager.Shutdown();
        }
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
#if DEBUG
            .WithDeveloperTools()
#endif
            .UseR3(ReportException)
            .HandleUIThreadException(ReportException);
}
