using ObservableCollections;

namespace Euterpe.Features.Logging;

[Route("/logging/app", DisplayName = Panel_Logging_AppLog, Order = 0)]
[AppSingleton]
public sealed class AppLogPanelViewModel : ViewModelBase
{
    public INotifyCollectionChangedSynchronizedViewList<LogMessage> LogMessagesView => LiveLogService.LogMessagesView;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(AppLogPanelViewModel));
    }

    #region Injections

    public required LiveLogService LiveLogService { get; init; }
    public required ILogger<AppLogPanelViewModel> Logger { get; init; }

    #endregion Injections
}
