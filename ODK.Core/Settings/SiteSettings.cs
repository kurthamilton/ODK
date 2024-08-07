﻿namespace ODK.Core.Settings;

public class SiteSettings : IVersioned, IDatabaseEntity
{    
    public int DefaultTrialPeriodMonths { get; set; }

    public string GoogleMapsApiKey { get; set; } = "";

    public Guid Id { get; set; }

    public string InstagramScraperUserAgent { get; set; } = "";

    public string RecaptchaSecretKey { get; set; } = "";

    public string RecaptchaSiteKey { get; set; } = "";

    public byte[] Version { get; set; } = [];
}
