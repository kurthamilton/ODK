namespace ODK.Core.Settings;

public class SiteSettings : IVersioned, IDatabaseEntity
{    
    public int DefaultTrialPeriodMonths { get; set; }

    public string GoogleMapsApiKey { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InstagramScraperUserAgent { get; set; } = string.Empty;

    public string RecaptchaSecretKey { get; set; } = string.Empty;

    public string RecaptchaSiteKey { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];
}
