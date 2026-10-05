using System.IO.Pipes;

namespace Euterpe;

internal static class ActivationPipe
{
    private const string PipeName = $"{AppName}-Activation";
    private const string BootstrapLogFile = "bootstrap.log";

    private static readonly CancellationTokenSource ListenCts = new();

    internal static void Send(string argument)
    {
        try
        {
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

    internal static void StartListening() => ListenAsync(ListenCts.Token).SafeFireAndForget();

    internal static void StopListening()
    {
        ListenCts.Cancel();
        ListenCts.Dispose();
    }

    private static async Task ListenAsync(CancellationToken ct)
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
                    Dispatcher.UIThread.Post(() => IocContainer.Resolve<SystemActivationService>().HandleActivation(argument));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                IocContainer.Resolve<ILogger<App>>().LogError(ex, "Activation pipe listener failed");
            }
        }
    }

    private static void LogBootstrapException(Exception ex)
    {
        try
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}";
            File.AppendAllText(Path.Combine(LocalAppDataFolder, BootstrapLogFile), message);
        }
        catch
        {
            // Nothing we can do here
        }
    }
}
