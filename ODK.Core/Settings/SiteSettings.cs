namespace ODK.Core.Settings;

public class SiteSettings : IVersioned, IDatabaseEntity
{
    public string DefaultFromEmailAddress { get; set; } = "";

    public string DefaultFromEmailName { get; set; } = "";

    public int DefaultTrialPeriodMonths { get; set; }

    public string GoogleMapsApiKey { get; set; } = "";

    public Guid Id { get; set; }

    public string InstagramPassword { get; set; } = "";

    public string InstagramScraperUserAgent { get; set; } = "";

    public string InstagramUsername { get; set; } = "";

    public string RecaptchaSecretKey { get; set; } = "";

    public string RecaptchaSiteKey { get; set; } = "";

    public byte[] Version { get; set; } = [];
}
