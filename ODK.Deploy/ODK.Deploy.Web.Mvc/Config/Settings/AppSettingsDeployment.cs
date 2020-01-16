namespace ODK.Deploy.Web.Mvc.Config.Settings
{
    public class AppSettingsDeployment
    {
        public string BuildPath { get; set; }

        public string Name { get; set; }

        public string OfflineFile { get; set; }

        public string[] PreservedPaths { get; set; } = new string[0];

        public string RemotePath { get; set; }

        public string Server { get; set; }
    }
}
