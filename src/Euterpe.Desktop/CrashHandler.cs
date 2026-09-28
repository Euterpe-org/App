using static Euterpe.IocContainer;

namespace Euterpe;

internal static class CrashHandler
{
    private const int ReportStormLimit = 100;
    private static int ReportsSinceLastDialogClose;
    private static bool CrashWindowShowing;

    internal static void ReportException(Exception ex)
    {
        Resolve<ILogger<App>>().LogCritical(ex, "Unhandled exception");

        if (Resolve<Config>().IgnoreException)
        {
            return;
        }

        if (Interlocked.Increment(ref ReportsSinceLastDialogClose) > ReportStormLimit)
        {
            Environment.Exit(1);
        }

        Dispatcher.UIThread.Post(() => ShowCrashDialogAsync(ex).SafeFireAndForget());
    }

    private static async Task ShowCrashDialogAsync(Exception ex)
    {
        if (CrashWindowShowing)
        {
            return;
        }

        CrashWindowShowing = true;
        StartCrashLogUpload();

        bool shouldContinue;
        try
        {
            var vm = Resolve<CrashWindowViewModel>();
            vm.SetException(ex);

            shouldContinue = await Resolve<IDialogService>()
                .ShowWindowDialogAsync<CrashWindow, CrashWindowViewModel, bool>(vm)
                .ConfigureAwait(true);
        }
        catch
        {
            shouldContinue = false;
        }
        finally
        {
            CrashWindowShowing = false;
            Interlocked.Exchange(ref ReportsSinceLastDialogClose, 0);
        }

        if (!shouldContinue)
        {
            Environment.Exit(1);
        }
    }

    private static void StartCrashLogUpload()
    {
        try
        {
            Resolve<ICrashLogUploadService>()
                .UploadAppLogAsync()
                .SafeFireAndForget(ex => Resolve<ILogger<App>>().LogWarning(ex, "Failed to start crash log upload"));
        }
        catch (Exception ex)
        {
            Resolve<ILogger<App>>().LogWarning(ex, "Failed to start crash log upload");
        }
    }
}
