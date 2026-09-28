using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Wizard;

[Register]
public sealed partial class WizardDialogViewModel : ViewModelBase, IDialogContext
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(IsLastPage))]
    public partial int CurrentPageIndex { get; set; }

    public IReadOnlyList<SetupPageViewModelBase> Pages { get; private set; } = null!;

    public SetupPageViewModelBase CurrentPage => Pages[CurrentPageIndex];

    public bool CanGoBack => CurrentPage.CanGoBack && CurrentPageIndex > 0;

    public bool IsLastPage => CurrentPageIndex == Pages.Count - 1;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public async Task PrepareForFullSetupAsync()
    {
        State.Reset();
        Pages = [GamePathPage, RolePage, ExecutionPage];

        CurrentPageIndex = 0;
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsLastPage));

        foreach (var page in Pages)
        {
            await page.InitializeAsync().ConfigureAwait(true);
        }

        CurrentPage.OnEnterAsync().SafeFireAndForget(ex => Logger.LogError(ex, "Wizard OnEnter failed"));
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(WizardDialogViewModel));
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (!IsLastPage)
        {
            CurrentPageIndex++;
            await CurrentPage.OnEnterAsync().ConfigureAwait(false);
            return;
        }

        GameConfig.SetupCompleted = true;
        Close();
    }

    [RelayCommand]
    private void Back() => CurrentPageIndex--;

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<WizardDialogViewModel> Logger { get; init; }
    public required ExecutionPageViewModel ExecutionPage { get; init; }
    public required GamePathPageViewModel GamePathPage { get; init; }
    public required RolePageViewModel RolePage { get; init; }
    public required SetupState State { get; init; }

    #endregion Injections
}
