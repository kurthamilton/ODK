using Microsoft.Extensions.DependencyInjection;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Remote.Web.Api.Config.Settings;

namespace ODK.Remote.Web.Api.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, AppSettings appSettings)
        {
            ConfigureServiceSettings(services, appSettings);
            ConfigureServices(services);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFileSystemRemoteClient, FileSystemRemoteClient>();
        }

        private static void ConfigureServiceSettings(IServiceCollection services, AppSettings settings)
        {
            services.AddSingleton(new FileSystemRemoteClientSettings
            {
                RootPath = settings.Paths.Root
            });
        }
    }
}
