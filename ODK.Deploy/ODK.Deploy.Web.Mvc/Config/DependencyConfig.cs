using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Data.Repositories;
using ODK.Deploy.Services.Deployments;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Services.Remote.FileSystem;
using ODK.Deploy.Services.Remote.Ftp;
using ODK.Deploy.Web.Mvc.Config.Settings;

namespace ODK.Deploy.Web.Mvc.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, AppSettings appSettings)
        {
            ConfigureDataSettings(services, appSettings);
            ConfigureData(services);
            ConfigureServiceSettings(services, appSettings);
            ConfigureServices(services);
        }

        private static void ConfigureData(IServiceCollection services)
        {
            services.AddScoped<IDeploymentRepository, DeploymentRepository>();
        }

        private static void ConfigureDataSettings(IServiceCollection services, AppSettings appSettings)
        {
            IReadOnlyCollection<Deployment> deployments = appSettings.Deployments.Select(x => new Deployment
            {
                BuildPath = x.BuildPath,
                Name = x.Name,
                OfflineFile = x.OfflineFile,
                PreservedPaths = x.PreservedPaths,
                RemotePath = x.RemotePath
            }).ToArray();

            services.AddSingleton(deployments);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDeploymentService, DeploymentService>();
            services.AddScoped<IFileSystemRemoteClient, FileSystemRemoteClient>();
            services.AddScoped<IFtpRemoteClient, FtpRemoteClient>();
            services.AddScoped<IRemoteService, RemoteService>();
        }

        private static void ConfigureServiceSettings(IServiceCollection services, AppSettings settings)
        {
            services.AddSingleton(new FileSystemRemoteClientSettings
            {
                RootPath = settings.Paths.Root
            });

            services.AddSingleton(new FtpRemoteClientSettings
            {
                Password = settings.Ftp.Password,
                Server = settings.Ftp.Server,
                UserName = settings.Ftp.UserName
            });

            services.AddSingleton(new RemoteServiceSettings
            {
                RemoteBackup = settings.Paths.RemoteBackup,
                RemoteDeploy = settings.Paths.RemoteDeploy,
                Type = settings.RemoteType
            });
        }
    }
}
