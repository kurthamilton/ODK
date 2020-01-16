using ODK.Deploy.Core.Servers;
using ODK.Deploy.Services.Remote;

namespace ODK.Deploy.Web.Mvc.Config.Settings
{
    public class AppSettingsServer
    {
        public string Name { get; set; }

        public AppSettingsFileSystem FileSystem { get; set; }

        public AppSettingsFtp Ftp { get; set; }

        public AppSettingsPaths Paths { get; set; } = new AppSettingsPaths();

        public AppSettingsRest Rest { get; set; }

        public ServerType Type { get; set; }
    }
}
