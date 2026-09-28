using System.IO.Pipes;
using NLog;
using Velopack;
using static Euterpe.CrashHandler;
using static Euterpe.IocContainer;

namespace Euterpe;

public static class Bootstrapper
{
    private const string PipeName = $"{AppName}-Activation";
    private const string BootstrapLogFile = "bootstrap.log";

    private static readonly CancellationTokenSource ActivationPipeCts = new();

    public static void Run<TPlatform>(string[] args) where TPlatform : IPlatformServices
    {
        VelopackApp.Build().Run();

        using var mutex = new Mutex(true, AppName, out var createdNew);
        if (!createdNew)
        {
            if (args is not [])
            {
                SendArgsToPrimaryInstance(args);
            }

            return;
        }

        Directory.CreateDirectory(AppDataFolder);
        CleanupLogFiles();
        ConfigureContainer<TPlatform>();
        StartActivationPipeServer();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            StopActivationPipeServer();
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

    private static void StartActivationPipeServer()
    {
        ListenForActivationPipeAsync(ActivationPipeCts.Token).SafeFireAndForget();
    }

    private static void StopActivationPipeServer()
    {
        ActivationPipeCts.Cancel();
        ActivationPipeCts.Dispose();
    }

    private static void CleanupLogFiles()
    {
        try
        {
            if (!Directory.Exists(AppLogsFolder))
            {
                return;
            }

            var logFiles = Directory.EnumerateFiles(AppLogsFolder, "*.log").OrderDescending().Skip(30);
            foreach (var logFile in logFiles)
            {
                File.Delete(logFile);
            }
        }
        catch (Exception ex)
        {
            LogBootstrapException(ex);
        }
    }

    private static void SendArgsToPrimaryInstance(string[] args)
    {
        try
        {
            var argument = args[0];
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(3000);
            using var writer = new StreamWriter(client);
            writer.Write(argument);
            writer.Flush();
        }
        catch (Exception ex)
        {
            LogBootstrapException(ex);
        }
    }

    private static async Task ListenForActivationPipeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await using (server.ConfigureAwait(false))
                {
                    await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    using var reader = new StreamReader(server);
                    var argument = await reader.ReadToEndAsync(ct).ConfigureAwait(false);

                    if (!argument.IsNullOrEmpty())
                    {
                        Dispatcher.UIThread.Post(() => IocContainer.Resolve<SystemActivationService>().HandleActivation(argument));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                LogBootstrapException(ex);
            }
        }
    }

    private static void LogBootstrapException(Exception ex)
    {
        try
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}";
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, BootstrapLogFile), message);
        }
        catch
        {
            // Nothing we can do here
        }
    }
}
