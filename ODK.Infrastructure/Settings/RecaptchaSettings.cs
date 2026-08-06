namespace ODK.Infrastructure.Settings;

public class RecaptchaSettings
{
    /// <summary>
    /// Set to false to skip reCAPTCHA entirely (no widget, no verification). Used by the e2e environment,
    /// which has no keys and drives the forms with an automation browser.
    /// </summary>
    public bool Enabled { get; init; } = true;

    public required double ScoreThreshold { get; init; }

    public required string SecretKey { get; init; }

    public required string SiteKey { get; init; }

    public required string VerifyUrl { get; init; }
}