namespace ODK.Core.Settings
{
    public class SiteSettings : IVersioned
    {
        public SiteSettings(string googleMapsApiKey, string instagramUsername, string instagramPassword, 
            string instagramScraperUserAgent, bool scrapeInstagram, long version, string recaptchaSiteKey, 
            string recaptchaSecretKey)
        {
            GoogleMapsApiKey = googleMapsApiKey;
            InstagramPassword = instagramPassword;
            InstagramScraperUserAgent = instagramScraperUserAgent;
            InstagramUsername = instagramUsername;
            ScrapeInstagram = scrapeInstagram;
            Version = version;
            RecaptchaSecretKey = recaptchaSecretKey;
            RecaptchaSiteKey = recaptchaSiteKey;
        }

        public string GoogleMapsApiKey { get; }

        public string InstagramPassword { get; }

        public string InstagramScraperUserAgent { get; }

        public string InstagramUsername { get; }

        public string RecaptchaSecretKey { get; }

        public string RecaptchaSiteKey { get; }

        public bool ScrapeInstagram { get; }

        public long Version { get; }
    }
}
