namespace ODK.Core.Settings;

public class SiteSettings : IVersioned
{
    public string GoogleMapsApiKey { get; set; } = "";

    public string InstagramPassword { get; set; } = "";

    public string InstagramScraperUserAgent { get; set; } = "";

    public string InstagramUsername { get; set; } = "";

    public string RecaptchaSecretKey { get; set; } = "";

    public string RecaptchaSiteKey { get; set; } = "";

    public bool ScrapeInstagram { get; set; }

    public byte[] Version { get; set; } = [];
}
