namespace ODK.Core.Settings
{
    public class SiteSettings : IVersioned
    {
        public SiteSettings(string googleMapsApiKey, long version)
        {
            GoogleMapsApiKey = googleMapsApiKey;
            Version = version;
        }

        public string GoogleMapsApiKey { get; }

        public long Version { get; }
    }
}
