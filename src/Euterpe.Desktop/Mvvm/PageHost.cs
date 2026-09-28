namespace Euterpe.Mvvm;

public sealed partial class PageHost : ObservableObject
{
    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial ViewModelBase Content { get; set; } = null!;

    partial void OnIsActiveChanged(bool value)
    {
        if (value)
        {
            Content.InitializeAsync().SafeFireAndForget();
        }
    }

    partial void OnContentChanged(ViewModelBase value)
    {
        if (IsActive)
        {
            value.InitializeAsync().SafeFireAndForget();
        }
    }

    public static PageHost PerGame<T>() where T : ViewModelBase
    {
        var host = new PageHost();
        IocContainer.GameScopeObservable.Subscribe(host, static (scope, h) => h.Content = scope.Resolve<T>());
        return host;
    }

    public static PageHost App(ViewModelBase content) => new() { Content = content };
}
