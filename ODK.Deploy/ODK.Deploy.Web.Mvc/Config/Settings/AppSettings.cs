namespace ODK.Deploy.Web.Mvc.Config.Settings
{
    public class AppSettings
    {
        public AppSettingsDeployment[] Deployments { get; set; } = new AppSettingsDeployment[0];

        public AppSettingsServer[] Servers { get; set; } = new AppSettingsServer[0];
    }
}
