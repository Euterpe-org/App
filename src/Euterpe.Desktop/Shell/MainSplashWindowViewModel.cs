using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Shell;

[Register]
[AppSingleton]
public sealed class MainSplashWindowViewModel : ViewModelBase, IDialogContext
{
    public const string DialogHostId = "SplashDialogHost";

    public AsyncManualResetEvent Ready { get; } = new(false);

    public void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<object?>? RequestClose;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        if (!await AuthService.IsServerHealthyAsync().ConfigureAwait(true))
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_Server_Unavailable).ConfigureAwait(false);
            Environment.Exit(0);
            return;
        }

        var restored = await AuthService.RestoreSessionAsync().ConfigureAwait(true);
        if (!restored)
        {
            await EnsureLoggedInAsync().ConfigureAwait(true);
        }

        await AuthService.Ready.WaitAsync().ConfigureAwait(true);

#if RELEASE
        await CheckAppUpdateAsync().ConfigureAwait(true);
#endif

        Ready.Set();
        Logger.LogInformation("{ViewModel} Initialized", nameof(MainSplashWindowViewModel));
        Close();
    }

    /// <summary>
    ///     Drive the login flow until it succeeds, prompting to retry or quit on each failure.
    ///     Exits the process if the user chooses to quit.
    /// </summary>
    private async Task EnsureLoggedInAsync()
    {
        while (true)
        {
            await AuthService.LoginAsync().ConfigureAwait(true);
            if (AuthService.Ready.IsSet)
            {
                return;
            }

            var result = await MessageBoxService.WarningConfirmAsync(MessageBox_Content_Login_Failed).ConfigureAwait(true);
            if (result)
            {
                continue;
            }

            Environment.Exit(0);
        }
    }

#if RELEASE
    private async Task CheckAppUpdateAsync()
    {
        string? newVersion;

        try
        {
            newVersion = await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Update check failed during splash");
            await MessageBoxService.ErrorAsync(MessageBox_Content_Update_Check_Failed).ConfigureAwait(false);

            Environment.Exit(0);
            return;
        }

        if (newVersion is null)
        {
            return;
        }

        if (!await UpdateDialogService.ShowAsync(newVersion, DialogHostId).ConfigureAwait(true))
        {
            Environment.Exit(0);
        }
    }
#endif

    #region Injections

    public required UpdateDialogService UpdateDialogService { get; init; }
    public required IAuthService AuthService { get; init; }
    public required ILogger<MainSplashWindowViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IUpdateService UpdateService { get; init; }

    #endregion Injections
}
