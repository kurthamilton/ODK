using Microsoft.Extensions.DependencyInjection;
using ODK.Deploy.Core.Servers;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Remote.Web.Api.Config.Settings;

namespace ODK.Remote.Web.Api.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, AppSettingsPaths settings)
        {
            ConfigureServices(services, settings);
        }

        private static void ConfigureServices(IServiceCollection services, AppSettingsPaths settings)
        {
            services.AddScoped<IFileSystemRemoteClient, FileSystemRemoteClient>();

            services.AddSingleton(new FileSystemSettings
            {
                RootPath = settings.Root
            });
        }
    }
}
