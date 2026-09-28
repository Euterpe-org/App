using Velopack.Sources;

namespace Euterpe.Extensions;

public static partial class ServiceExtensions
{
    extension(ContainerBuilder builder)
    {
        public void RegisterDesktopServices()
        {
            builder.RegisterType<DialogService>().As<IDialogService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MessageBoxService>().As<IMessageBoxService>().SingleInstance();
            builder.RegisterType<NotificationService>().As<INotificationService>().As<INotificationServiceWiring>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ResourceService>().As<IResourceService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UpdateService>().As<IUpdateService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<VelopackFileDownloader>().As<IFileDownloader>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<TopLevelProxy>().PropertiesAutowired().SingleInstance();
        }

        public void RegisterInternalServices()
        {
            // Self Services
            builder.RegisterType<AppInitializer>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<SystemActivationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameSwitcher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UpdateDialogService>().PropertiesAutowired().SingleInstance();

            builder.RegisterType<UpdateDialogViewModel>().PropertiesAutowired().InstancePerDependency();

            // Auto Activate Services
            builder.RegisterType<LiveLogService>().PropertiesAutowired().SingleInstance().AutoActivate();
        }

        public void RegisterPerGameAppServices()
        {
            builder.RegisterType<ProgressDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<SetupDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ShareImportDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}
