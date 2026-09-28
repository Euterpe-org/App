using Euterpe.Core.Audio.Codecs;
using Euterpe.Core.Http.Handlers;
using Euterpe.Core.Http.Listeners;
using Euterpe.Core.Http.Resilience;
using NLog.Extensions.Logging;
using Refit;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;

namespace Euterpe.Core.Extensions;

public static class CoreServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void RegisterLogger()
        {
            var liveLogTarget = new LiveLogTarget();
            var configuration = AppLoggingConfiguration.Create(liveLogTarget);

            services.AddSingleton(liveLogTarget);
            services.AddLogging(x =>
            {
                x.ClearProviders();
                x.SetMinimumLevel(AppLoggingConfiguration.MinimumMicrosoftLogLevel);
                x.AddNLog(configuration);
            });
        }

        public void RegisterHttpClients()
        {
            services.AddTransient<XRequestIdHandler>();
            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<LoggingHandler>();
            services.AddTransient<ServerErrorHandler>();
            services.AddTransient<TokenQueryHandler>();

            services.AddSingleton<ServerErrorNotifier>();

            services.AddSingleton<Func<DownloadService>>(sp =>
            {
                var handler = sp.GetRequiredService<TokenQueryHandler>();
                handler.InnerHandler = new SocketsHttpHandler();

                return () => new DownloadService(new DownloadConfiguration
                {
                    ChunkCount = 8,
                    MaxTryAgainOnFailure = 4,
                    ParallelDownload = true,
                    BlockTimeout = 3000,
                    CustomHttpMessageHandlerFactory = () => handler
                });
            });

            services.ConfigureHttpClientDefaults(builder =>
            {
                builder.AddHttpMessageHandler<LoggingHandler>();
                builder.AddHttpMessageHandler<ServerErrorHandler>();
            });
            services.AddHttpClient();
            services.AddHttpClient<EuterpeDownloadClient>().AddHttpMessageHandler<TokenQueryHandler>();

            services.AddEuterpeRefitClient<IEuterpeAccountClient>(nameof(EuterpeApi.Account), EuterpeApi.Account.BasePath, true)
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureApi);
            services.AddEuterpeRefitClient<IEuterpeAuthClient>(nameof(EuterpeApi.Auth), EuterpeApi.Auth.BasePath)
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureApi);
            services.AddEuterpeRefitClient<IEuterpeChartClient>(nameof(EuterpeApi.Charts), EuterpeApi.Charts.BasePath, true)
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureApi);
            services.AddEuterpeRefitClient<IEuterpeCreditsClient>(nameof(EuterpeApi.Public), EuterpeApi.Public.BasePath)
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureApi);
            services.AddEuterpeRefitClient<IEuterpeDistributionClient>(nameof(EuterpeApi.Distribution), EuterpeApi.Distribution.BasePath, true)
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureApi);
            services.AddEuterpeRefitClient<IEuterpeLogClient>(nameof(EuterpeApi.Logs), EuterpeApi.Logs.BasePath, true);
            services.AddEuterpeRefitClient<IEuterpeModClient>(nameof(EuterpeApi.Mods), EuterpeApi.Mods.BasePath, true)
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureApi);
            services.AddEuterpeRefitClient<IEuterpeTelemetryClient>(nameof(EuterpeApi.Telemetry), EuterpeApi.Telemetry.BasePath);

            services.AddRefitGeneratedClient<IEuterpeHealthClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(EuterpeWeb.BaseUrl))
                .AddStandardResilienceHandler(HttpResiliencePolicies.ConfigureHealthCheck);
        }
    }

    extension(ContainerBuilder builder)
    {
        public void RegisterAppCoreServices()
        {
            builder.RegisterType<AuthState>().SingleInstance();
            builder.RegisterType<PlaybackState>().SingleInstance();
            builder.RegisterType<Config>().PropertiesAutowired().SingleInstance();

            builder.RegisterType<MuseDashConfig>().AsSelf().SingleInstance();
            builder.RegisterType<MuseDash2Config>().AsSelf().SingleInstance();

            builder.Register<AudioEngine>(_ =>
            {
                var engine = new MiniAudioEngine();
                engine.RegisterCodecFactory(new VorbisCodecFactory());
                return engine;
            }).SingleInstance();

            builder.RegisterType<AppDownloadManager>().As<IAppDownloadManager>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AppLocalService>().As<IAppLocalService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AppSettingService>().As<IAppSettingService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ArchiveService>().As<IArchiveService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AudioPlayerService>().As<IAudioPlayerService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AuthService>().As<IAuthService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<CrashLogUploadService>().As<ICrashLogUploadService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<FileSystemService>().As<IFileSystemService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ImageCacheService>().As<IRemoteImageLoader>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MessagePackSerializationService>().As<IMessagePackSerializationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LoopbackCallbackListener>().As<ILoopbackCallbackListener>().InstancePerDependency();
            builder.RegisterType<TelemetryService>().As<ITelemetryService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<VdfSerializationService>().As<IVdfSerializationService>().PropertiesAutowired().SingleInstance();
        }

        public void RegisterPerGameCoreServices(GameId activeGame)
        {
            switch (activeGame)
            {
                case GameId.MuseDash2:
                    builder.Register<GameConfig>(static ctx => ctx.Resolve<MuseDash2Config>()).InstancePerLifetimeScope();
                    break;
                case GameId.MuseDash:
                    builder.Register<GameConfig>(static ctx => ctx.Resolve<MuseDashConfig>()).InstancePerLifetimeScope();
                    break;
            }

            builder.RegisterType<SetupState>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<ChartLocalService>().As<IChartLocalService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ChartManageService>().As<IChartManageService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MigrationService>().As<IMigrationService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<DependencyAcquireService>().As<IDependencyAcquireService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameDownloadManager>().As<IGameDownloadManager>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameLaunchService>().As<IGameLaunchService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameLocalService>().As<IGameLocalService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GamePathService>().As<IGamePathService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameSettingService>().As<IGameSettingService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ModLocalService>().As<IModLocalService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameShareService>().As<IGameShareService>().PropertiesAutowired().InstancePerLifetimeScope();

            // Setup Steps
            builder.RegisterType<ChartingToolStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MigrationStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<DotNetRuntimeStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<DotNetSdkStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<EnvVariableStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<EssentialModsStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MelonLoaderStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ModTemplateStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<UninstallConflictsStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}
