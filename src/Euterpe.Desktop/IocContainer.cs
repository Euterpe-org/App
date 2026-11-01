using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe;

public static class IocContainer
{
    public const string GameScopeTag = "Game";

    private static readonly Dictionary<GameId, ILifetimeScope> GameScopes = new();
    private static IContainer Root { get; set; } = null!;
    private static BehaviorSubject<ILifetimeScope> GameScopeSubject { get; set; } = null!;

    public static Observable<ILifetimeScope> GameScopeObservable => GameScopeSubject;

    private static ILifetimeScope GameScope => GameScopeSubject.Value;

    public static T Resolve<T>() where T : notnull => GameScope.Resolve<T>();

    internal static void SetTestScope(ILifetimeScope scope) =>
        GameScopeSubject = new BehaviorSubject<ILifetimeScope>(scope);

    public static void ConfigureContainer<TPlatform>() where TPlatform : IPlatformServices
    {
        var services = new ServiceCollection();
        services.RegisterLogger();
        services.RegisterHttpClients();

        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterDesktopServices();
        TPlatform.RegisterServices(builder);
        builder.RegisterInternalServices();
        builder.RegisterAppViewModels();

        builder.Register(static _ => GameScopeSubject).AsSelf().As<Observable<ILifetimeScope>>().SingleInstance();

        builder.Register<Func<GameId, IGameLocalService>>(static _ => static game => GetOrBuildGameScope(game).Resolve<IGameLocalService>())
            .SingleInstance();

        builder.Populate(services);
        Root = builder.Build();

        Root.Resolve<IAppSettingService>().Load();

        var activeGameScope = GetOrBuildGameScope(Root.Resolve<Config>().ActiveGame);
        GameScopeSubject = new BehaviorSubject<ILifetimeScope>(activeGameScope);
    }

    public static void ActivateGame(GameId game)
    {
        var scope = GetOrBuildGameScope(game);
        Dispatcher.UIThread.Post(() => GameScopeSubject.OnNext(scope));
    }

    private static ILifetimeScope GetOrBuildGameScope(GameId game)
    {
        if (!GameScopes.TryGetValue(game, out var scope))
        {
            scope = BuildGameScope(game);
            GameScopes[game] = scope;
        }

        return scope;
    }

    private static ILifetimeScope BuildGameScope(GameId game) =>
        Root.BeginLifetimeScope(GameScopeTag, builder =>
        {
            builder.RegisterPerGameCoreServices(game);
            builder.RegisterPerGameAppServices();
            builder.RegisterPerGameViewModels();
        });
}
