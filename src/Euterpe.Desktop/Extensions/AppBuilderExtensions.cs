namespace Euterpe.Extensions;

public static class AppBuilderExtensions
{
    public static AppBuilder HandleUIThreadException(this AppBuilder builder, Action<Exception> handler)
    {
        return builder.AfterSetup(_ => Dispatcher.UIThread.UnhandledException += (_, args) =>
        {
            handler(args.Exception);
            args.Handled = true;
        });
    }
}
