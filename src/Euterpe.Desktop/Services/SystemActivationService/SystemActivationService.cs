namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    public async Task SetupAsync()
    {
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Process path is null");
        await AssociationSetup.RegisterAsync(processPath).ConfigureAwait(false);
    }

    public void HandleStartupArgs(string[] args)
    {
        if (args is [])
        {
            return;
        }

        HandleActivation(args[0]);
    }

    public void HandleActivation(string argument)
    {
        Logger.LogInformation("Activation received: {Argument}", argument);

        if (Uri.TryCreate(argument, UriKind.Absolute, out var parsed) && parsed.Scheme is ISystemAssociationSetup.DeepLinkScheme)
        {
            HandleDeepLink(parsed);
            return;
        }

        if (GetEpkPath(argument) is { } epkFilePath)
        {
            HandleEpkFile(epkFilePath);
            return;
        }

        Logger.LogWarning("Unhandled activation: {Argument}", argument);
    }

    #region Injections

    public required BehaviorSubject<ILifetimeScope> GameScope { get; init; }
    public required NavigationService NavigationService { get; init; }
    public required INotificationService NotificationService { get; init; }
    public required ILogger<SystemActivationService> Logger { get; init; }
    public required ISystemAssociationSetup AssociationSetup { get; init; }

    #endregion Injections
}
