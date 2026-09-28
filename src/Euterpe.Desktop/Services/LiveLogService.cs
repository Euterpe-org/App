using ObservableCollections;

namespace Euterpe.Services;

public sealed class LiveLogService
{
    public readonly INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView;
    private readonly ObservableFixedSizeRingBuffer<LogMessage> _logMessages = new(50);

    public LiveLogService(LiveLogTarget target)
    {
        target.OnLogMessageReceived += logMessage => _logMessages.AddLast(logMessage);

        LogMessagesView = _logMessages.CreateView(x => x).ToNotifyCollectionChanged();
    }
}
