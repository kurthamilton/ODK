namespace ODK.Services.Integrations.Recaptcha;

public class RecaptchaServiceSettings
{
    public double ScoreThreshold { get; set; }

    public string VerifyUrl { get; set; } = string.Empty;
}
