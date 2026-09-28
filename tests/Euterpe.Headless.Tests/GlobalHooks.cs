using Autofac;
using Euterpe.Abstractions;

namespace Euterpe.Headless.Tests;

public static class GlobalHooks
{
    [Before(TestSession)]
    public static void BootstrapTestIoc()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ResourceService>().As<IResourceService>().SingleInstance();
        var container = builder.Build();
        IocContainer.SetTestScope(container.BeginLifetimeScope());
    }

    [After(TestSession)]
    public static ValueTask DisposeHeadlessSession() =>
        HeadlessTest.SessionLazy.IsValueCreated
            ? HeadlessTest.SessionLazy.Value.DisposeAsync()
            : ValueTask.CompletedTask;
}
