namespace Euterpe.Services;

public sealed partial class GameSwitcher : ObservableObject
{
    private readonly AsyncExclusiveLock _switchLock = new();

    [ObservableProperty]
    public partial bool CanSwitch { get; set; } = true;

    public async Task SwitchAsync(GameId target)
    {
        if (target == Config.ActiveGame)
        {
            return;
        }

        if (!await _switchLock.TryAcquireAsync(TimeSpan.Zero).ConfigureAwait(false))
        {
            Logger.LogInformation("Switch to {Target} ignored — another switch is in progress", target);
            return;
        }

        var previous = Config.ActiveGame;
        try
        {
            CanSwitch = false;

            Logger.LogInformation("Switching active game from {Previous} to {Target}", previous, target);
            Config.ActiveGame = target;
            IocContainer.ActivateGame(target);
        }
        catch
        {
            Config.ActiveGame = previous;
            throw;
        }
        finally
        {
            CanSwitch = true;
            _switchLock.Release();
        }
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<GameSwitcher> Logger { get; init; }

    #endregion Injections
}
