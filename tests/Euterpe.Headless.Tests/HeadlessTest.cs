namespace Euterpe.Headless.Tests;

public abstract class HeadlessTest
{
    internal static readonly Lazy<HeadlessUnitTestSession> SessionLazy =
        new(() => HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder)));

    protected static HeadlessUnitTestSession Session => SessionLazy.Value;

    protected static Task RunOnUI(Action action) =>
        Session.Dispatch(action, CancellationToken.None);

    protected static Task RunOnUI(Func<Task> func) =>
        Session.Dispatch(
            async () =>
            {
                await func();
                return 0;
            },
            CancellationToken.None);

    protected static Task<T> RunOnUI<T>(Func<Task<T>> func) =>
        Session.Dispatch(func, CancellationToken.None);

    protected static async Task WaitUntil(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            if (condition())
            {
                return;
            }

            await Task.Delay(5);
        }
    }
}
