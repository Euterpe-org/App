using System.Reactive.Concurrency;
using SingleAssignmentDisposable = System.Reactive.Disposables.SingleAssignmentDisposable;
using StableCompositeDisposable = System.Reactive.Disposables.StableCompositeDisposable;

namespace Euterpe.Reactive;

internal sealed class AvaloniaScheduler : LocalScheduler
{
    public static AvaloniaScheduler Instance { get; } = new();

    private AvaloniaScheduler()
    {
    }

    public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        // Run inline when already on the UI thread: ObserveOn gates scheduling behind its work queue, so an inline run can never reorder ahead of a pending post.
        if (Dispatcher.UIThread.CheckAccess())
        {
            return action(this, state);
        }

        var disposable = new SingleAssignmentDisposable();
        Dispatcher.UIThread.Post(
            () =>
            {
                if (!disposable.IsDisposed)
                {
                    disposable.Disposable = action(this, state);
                }
            },
            DispatcherPriority.Normal);
        return disposable;
    }

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        var delay = Scheduler.Normalize(dueTime);
        if (delay == TimeSpan.Zero)
        {
            return Schedule(state, action);
        }

        var disposable = new SingleAssignmentDisposable();
        var timer = DispatcherTimer.RunOnce(
            () =>
            {
                if (!disposable.IsDisposed)
                {
                    disposable.Disposable = action(this, state);
                }
            },
            delay,
            DispatcherPriority.Normal);
        return StableCompositeDisposable.Create(timer, disposable);
    }
}
