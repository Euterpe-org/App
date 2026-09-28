namespace Euterpe.Mvvm;

public abstract partial class RootNavViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial NavItem? SelectedItem { get; set; }

    public abstract IReadOnlyList<NavItem> NavItems { get; }
    public abstract IReadOnlyList<PageHost> Pages { get; }

    public required NavigationService NavigationService { get; init; }

    protected abstract PageHost ResolveRoute(string route);

    partial void OnSelectedItemChanged(NavItem? value)
    {
        if (value is null)
        {
            return;
        }

        var next = ResolveRoute(value.NavigateKey);
        foreach (var page in Pages)
        {
            page.IsActive = ReferenceEquals(page, next);
        }

        NavigationService.NotifyNavigated(value.NavigateKey);
    }

    protected override Task OnInitializeAsync()
    {
        SelectedItem ??= NavItems[0];
        return base.OnInitializeAsync();
    }
}
