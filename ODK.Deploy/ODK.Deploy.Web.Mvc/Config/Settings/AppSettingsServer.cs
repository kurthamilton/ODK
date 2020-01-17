using ODK.Deploy.Core.Servers;

namespace ODK.Deploy.Web.Mvc.Config.Settings
{
    public class AppSettingsServer
    {
        public AppSettingsDeployment[] Deployments { get; set; }

        public AppSettingsFileSystem FileSystem { get; set; }

        public AppSettingsFtp Ftp { get; set; }

        public string Name { get; set; }

        public AppSettingsPaths Paths { get; set; } = new AppSettingsPaths();

        public AppSettingsRest Rest { get; set; }

        public ServerType Type { get; set; }
    }
}
