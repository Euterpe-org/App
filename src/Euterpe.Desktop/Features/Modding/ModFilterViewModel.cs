namespace Euterpe.Features.Modding;

public sealed partial class ModFilterViewModel : ObservableObject
{
    [ObservableProperty] public partial string? SearchText { get; set; }
    [ObservableProperty] public partial ModFilterType ModFilter { get; set; } = ModFilterType.All;

    public Observable<Unit> Changed => field ??= this.ObservePropertyChanges().DebounceSearch(nameof(SearchText));

    public bool Matches(ModDto mod)
    {
        if (!SearchText.IsNullOrEmpty()
            && !mod.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            && !mod.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ModFilter switch
        {
            ModFilterType.Installed => mod.IsLocal,
            ModFilterType.Enabled => mod is { IsDisabled: false, IsLocal: true },
            ModFilterType.Disabled => mod is { IsDisabled: true, IsLocal: true },
            ModFilterType.Outdated => mod.State is ModState.Outdated,
            ModFilterType.Incompatible => mod is { State: ModState.Incompatible, IsLocal: true },
            _ => true
        };
    }
}
