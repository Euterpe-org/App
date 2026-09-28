using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Setup;

[Register]
public sealed partial class RepairDialogViewModel : ViewModelBase, IDialogContext
{
    [ObservableProperty]
    public partial SetupPageViewModelBase Content { get; set; } = null!;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public Task PrepareForGamePathAsync()
    {
        State.Reset();
        return ShowAsync(GamePathPage);
    }

    public Task PrepareForOptionAsync(SetupOptionKinds kind)
    {
        State.Reset();

        foreach (var option in GameConfig.SetupOptions)
        {
            option.IsSelected = option.Kinds == kind;
        }

        return ShowAsync(ExecutionPage);
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(RepairDialogViewModel));
    }

    [RelayCommand]
    private void Apply() => Close();

    private async Task ShowAsync(SetupPageViewModelBase content)
    {
        Content = content;
        await content.InitializeAsync().ConfigureAwait(true);
        content.OnEnterAsync().SafeFireAndForget(ex => Logger.LogError(ex, "Repair OnEnter failed"));
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ExecutionPageViewModel ExecutionPage { get; init; }
    public required GamePathPageViewModel GamePathPage { get; init; }
    public required SetupState State { get; init; }
    public required ILogger<RepairDialogViewModel> Logger { get; init; }

    #endregion Injections
}
