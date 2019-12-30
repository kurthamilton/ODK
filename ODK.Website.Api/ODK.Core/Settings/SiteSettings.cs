namespace ODK.Core.Settings
{
    public class SiteSettings : IVersioned
    {
        public SiteSettings(string googleMapsApiKey, string instagramUsername, string instagramPassword, long version)
        {
            GoogleMapsApiKey = googleMapsApiKey;
            InstagramPassword = instagramPassword;
            InstagramUsername = instagramUsername;
            Version = version;
        }

        public string GoogleMapsApiKey { get; }

        public string InstagramPassword { get; }

        public string InstagramUsername { get; }

        public long Version { get; }
    }
}
