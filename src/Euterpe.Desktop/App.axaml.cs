using Avalonia.Markup.Xaml;
using static Euterpe.IocContainer;

namespace Euterpe;

public sealed class App : Application
{
    public App() => DataContext = Resolve<AppViewModel>();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Resolve<AppInitializer>().Run(this);
        base.OnFrameworkInitializationCompleted();
    }
}
