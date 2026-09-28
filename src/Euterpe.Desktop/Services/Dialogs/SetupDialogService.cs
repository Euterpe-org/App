using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Services;

public sealed class SetupDialogService
{
    public async Task ShowFullWizardAsync()
    {
        await WizardDialogViewModel.PrepareForFullSetupAsync().ConfigureAwait(true);
        await ShowDialogAsync<WizardDialog, WizardDialogViewModel>(WizardDialogViewModel, Wizard_Title_Welcome).ConfigureAwait(true);
    }

    public async Task ShowOptionRepairAsync(SetupOptionKinds kind)
    {
        await RepairDialogViewModel.PrepareForOptionAsync(kind).ConfigureAwait(true);
        await ShowDialogAsync<RepairDialog, RepairDialogViewModel>(RepairDialogViewModel, Setup_Title_SettingUp).ConfigureAwait(true);
    }

    public async Task ShowGamePathRepairAsync()
    {
        await RepairDialogViewModel.PrepareForGamePathAsync().ConfigureAwait(true);
        await ShowDialogAsync<RepairDialog, RepairDialogViewModel>(RepairDialogViewModel, Setup_Title_GamePath).ConfigureAwait(true);
    }

    private async Task ShowDialogAsync<TView, TViewModel>(TViewModel vm, string title)
        where TView : Control, new()
        where TViewModel : ViewModelBase, IDialogContext
    {
        var options = new OverlayDialogOptions
        {
            Title = title,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        GameSwitcher.CanSwitch = false;
        try
        {
            await DialogService.ShowOverlayAsync<TView, TViewModel>(vm, options, MainWindowViewModel.DialogHostId).ConfigureAwait(true);
        }
        finally
        {
            GameSwitcher.CanSwitch = true;
        }
    }

    #region Injections

    public required IDialogService DialogService { get; init; }
    public required GameSwitcher GameSwitcher { get; init; }
    public required WizardDialogViewModel WizardDialogViewModel { get; init; }
    public required RepairDialogViewModel RepairDialogViewModel { get; init; }

    #endregion Injections
}
