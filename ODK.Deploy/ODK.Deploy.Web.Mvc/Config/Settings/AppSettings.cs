using ODK.Deploy.Services.Remote;

namespace ODK.Deploy.Web.Mvc.Config.Settings
{
    public class AppSettings
    {
        public AppSettingsDeployment[] Deployments { get; set; } = new AppSettingsDeployment[0];

        public AppSettingsFtp Ftp { get; set; } = new AppSettingsFtp();

        public AppSettingsPaths Paths { get; set; } = new AppSettingsPaths();

        public RemoteType RemoteType { get; set; }
    }
}
