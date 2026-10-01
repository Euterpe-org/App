using Autofac;
using Euterpe.Core.Extensions;

namespace Euterpe.Tests.Core.Extensions;

[Category("CoreServiceExtensionsTests")]
[TestSubject(typeof(CoreServiceExtensions))]
public sealed partial class CoreServiceExtensionsTest
{
    [Test]
    [Arguments(GameId.MuseDash, typeof(MuseDashConfig))]
    [Arguments(GameId.MuseDash2, typeof(MuseDash2Config))]
    public async Task RegisterPerGameCoreServices_GameIdProvided_ResolvesMatchingGameConfig(GameId gameId, Type expectedConcrete)
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterPerGameCoreServices(gameId);

        await using var container = builder.Build();
        var resolved = container.Resolve<GameConfig>();

        await Assert.That(resolved.GetType()).IsEqualTo(expectedConcrete);
    }
}
