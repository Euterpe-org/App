using Autofac;
using Autofac.Extensions.DependencyInjection;
using Euterpe.Core.Extensions;
using Euterpe.Extensions;
using Euterpe.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Tests.Platforms;

[Category("PlatformServiceRegistrationTests")]
[TestSubject(typeof(PlatformServices))]
public sealed class PlatformServiceRegistrationTest
{
    [Test]
    public async Task RegisterServices_TwoGames_SharesAppServicesAndIsolatesGameServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.RegisterAppCoreServices();
        builder.RegisterDesktopServices();
        PlatformServices.RegisterServices(builder);
        using var root = builder.Build();
        using var museDash = root.BeginLifetimeScope(
            IocContainer.GameScopeTag,
            static scope => scope.RegisterPerGameCoreServices(GameId.MuseDash));
        using var museDash2 = root.BeginLifetimeScope(
            IocContainer.GameScopeTag,
            static scope => scope.RegisterPerGameCoreServices(GameId.MuseDash2));

        var firstGame = museDash.Resolve<IGamePathEnvironment>();
        var secondGame = museDash2.Resolve<IGamePathEnvironment>();

        using var assertions = Assert.Multiple();
        await Assert.That(ReferenceEquals(
            museDash.Resolve<IPlatformSecureStorage>(),
            museDash2.Resolve<IPlatformSecureStorage>())).IsTrue();
        await Assert.That(ReferenceEquals(firstGame, museDash.Resolve<IGamePathEnvironment>())).IsTrue();
        await Assert.That(ReferenceEquals(firstGame, secondGame)).IsFalse();
        await Assert.That(museDash.Resolve<GameConfig>().Id).IsEqualTo(GameId.MuseDash);
        await Assert.That(museDash2.Resolve<GameConfig>().Id).IsEqualTo(GameId.MuseDash2);
    }
}
