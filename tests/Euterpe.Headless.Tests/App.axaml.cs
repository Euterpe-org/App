using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Euterpe.Headless.Tests;

public sealed class App : Application
{
    public override void RegisterServices()
    {
        ApplicationLifetime = new ClassicDesktopStyleApplicationLifetime();
        base.RegisterServices();
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
