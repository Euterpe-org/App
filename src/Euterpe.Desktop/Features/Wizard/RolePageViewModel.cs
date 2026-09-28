namespace Euterpe.Features.Wizard;

[Register]
public sealed class RolePageViewModel : SetupPageViewModelBase
{
    private bool _applyingPreset;

    public IReadOnlyList<SetupOption> Options => GameConfig.SetupOptions;

    public static IReadOnlyList<WizardRole> Roles { get; } =
    [
        new(WizardIdentity.Player, "UserGroup", Wizard_Role_Player, Wizard_Role_Player_Description, "#2563EB"),
        new(WizardIdentity.Charter, "Language", Wizard_Role_Charter, Wizard_Role_Charter_Description, "#7B2CBF"),
        new(WizardIdentity.Modder, "Code", Wizard_Role_Modder, Wizard_Role_Modder_Description, "#047857"),
        new(WizardIdentity.Custom, "Setting", Wizard_Role_Custom, Wizard_Role_Custom_Description, "#B45309")
    ];

    public override LocalizedString Title => Wizard_Title_Welcome;

    public override bool CanGoBack => false;

    public WizardRole SelectedRole
    {
        get => Roles.First(r => r.Identity == ComputeIdentity());
        set => ApplyPreset(value.Identity);
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Options.Select(t => t.ObservePropertyChanged(x => x.IsSelected))
            .Merge()
            .Where(this, (_, self) => !self._applyingPreset)
            .Subscribe(this, (_, self) => self.OnPropertyChanged(nameof(SelectedRole)));

        Logger.LogInformation("{ViewModel} Initialized", nameof(RolePageViewModel));
    }

    private void ApplyPreset(WizardIdentity identity)
    {
        if (identity is WizardIdentity.Custom)
        {
            return;
        }

        _applyingPreset = true;
        try
        {
            var preset = GameConfig.WizardPresets[identity];
            foreach (var option in Options)
            {
                option.IsSelected = preset.HasFlag(option.Kinds);
            }
        }
        finally
        {
            _applyingPreset = false;
            OnPropertyChanged(nameof(SelectedRole));
        }
    }

    private WizardIdentity ComputeIdentity()
    {
        var selected = Options.Where(t => t.IsSelected)
            .Select(x => x.Kinds)
            .Aggregate(SetupOptionKinds.None, (mask, k) => mask | k);

        foreach (var (identity, preset) in GameConfig.WizardPresets)
        {
            if (preset == selected)
            {
                return identity;
            }
        }

        return WizardIdentity.Custom;
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<RolePageViewModel> Logger { get; init; }

    #endregion Injections
}
