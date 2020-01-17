using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ODK.Deploy.Core.Deployments;
using ODK.Deploy.Core.Servers;
using ODK.Deploy.Data.Repositories;
using ODK.Deploy.Services.Deployments;
using ODK.Deploy.Services.Remote;
using ODK.Deploy.Services.Servers;
using ODK.Deploy.Web.Mvc.Config.Settings;

namespace ODK.Deploy.Web.Mvc.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSingleton(appSettings);

            ConfigureDataSettings(services, appSettings);
            ConfigureData(services);
            ConfigureServices(services);
        }

        private static void ConfigureData(IServiceCollection services)
        {
            services.AddScoped<IDeploymentRepository, DeploymentRepository>();
            services.AddScoped<IServerRepository, ServerRepository>();
        }

        private static void ConfigureDataSettings(IServiceCollection services, AppSettings appSettings)
        {
            IReadOnlyCollection<Server> servers = appSettings.Servers
                .Select(x => new Server
                {
                    FileSystem = x.Type == ServerType.FileSystem
                        ? new FileSystemSettings { RootPath = x.FileSystem.RootPath }
                        : null,
                    Ftp = x.Type == ServerType.Ftp
                        ? new FtpSettings { Host = x.Ftp.Host, Password = x.Ftp.Password, UserName = x.Ftp.UserName }
                        : null,
                    Name = x.Name,
                    Type = x.Type,
                    Paths = new ServerPaths
                    {
                        Backup = x.Paths?.Backup,
                        Deploy = x.Paths?.Deploy
                    },
                    Rest = x.Type == ServerType.Rest
                        ? new RestSettings { AuthKey = x.Rest.AuthHeaderKey, Url = x.Rest.BaseUrl }
                        : null
                }).ToArray();

            services.AddSingleton(servers);

            List<Deployment> deployments = new List<Deployment>();
            foreach (AppSettingsServer server in appSettings.Servers)
            {
                IEnumerable<Deployment> serverDeployments = server
                    .Deployments
                    .Select(x => new Deployment
                    {
                        BuildPath = x.BuildPath,
                        Name = x.Name,
                        OfflineFile = x.OfflineFile,
                        PreservedPaths = x.PreservedPaths,
                        RemotePath = x.RemotePath,
                        Server = server.Name
                    });

                deployments.AddRange(serverDeployments);
            }

            services.AddSingleton<IReadOnlyCollection<Deployment>>(deployments);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDeploymentService, DeploymentService>();
            services.AddScoped<IRemoteService, RemoteService>();
            services.AddScoped<IServerService, ServerService>();
        }
    }
}
