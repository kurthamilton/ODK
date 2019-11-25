using System.Configuration;

namespace ODK.Umbraco.Settings
{
    public static class AppSettings
    {
        public static bool SuppressEmails { get; } = GetBooleanAppSetting("odk:suppressEmails");

        private static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static bool GetBooleanAppSetting(string key)
        {
            bool.TryParse(GetAppSetting(key), out bool result);
            return result;
        }
    }
}
