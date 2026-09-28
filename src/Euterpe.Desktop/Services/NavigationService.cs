namespace Euterpe.Services;

public sealed class NavigationService
{
    public AsyncManualResetEvent Ready { get; } = new(false);

    public string? CurrentRoute { get; private set; }

    #region Injections

    public required ILogger<NavigationService> Logger { get; init; }

    #endregion Injections

    public void NavigateTo(string route)
    {
        if (string.Equals(CurrentRoute, route, StringComparison.Ordinal))
        {
            Logger.LogDebug("Already at route: {Route}, skipping navigation", route);
            return;
        }

        var node = RouteTree.Root;

        while (node.Children.FirstOrDefault(x => route.StartsWith(x.Path, StringComparison.Ordinal)) is { } child)
        {
            child.Select?.Invoke();
            node = child;
        }

        NotifyNavigated(route);
    }

    public async Task NavigateToAsync(string route)
    {
        await Ready.WaitAsync().ConfigureAwait(true);
        NavigateTo(route);
    }

    public void NotifyNavigated(string route)
    {
        CurrentRoute = route;
        Logger.LogInformation("Navigated to: {Route}", route);
    }
}
