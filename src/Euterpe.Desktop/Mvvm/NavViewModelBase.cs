namespace Euterpe.Mvvm;

public abstract partial class NavViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial ViewModelBase Content { get; set; } = null!;

    [ObservableProperty]
    public partial NavItem? SelectedItem { get; set; }

    public abstract IReadOnlyList<NavItem> NavItems { get; }

    public required IComponentContext Container { get; init; }
    public required NavigationService NavigationService { get; init; }

    protected abstract ViewModelBase ResolveRoute(string route);

    partial void OnSelectedItemChanged(NavItem? value)
    {
        if (value is null)
        {
            return;
        }

        Content = ResolveRoute(value.NavigateKey);
        Content.InitializeAsync().SafeFireAndForget();

        NavigationService.NotifyNavigated(value.NavigateKey);
    }

    protected override Task OnInitializeAsync()
    {
        SelectedItem ??= NavItems[0];
        return base.OnInitializeAsync();
    }
}
