namespace ODK.Services.Integrations.Recaptcha;

public class RecaptchaServiceSettings
{
    public required double ScoreThreshold { get; init; }

    public required string SecretKey { get; init; }

    public required string SiteKey { get; init; }

    public required string VerifyUrl { get; init; }
}