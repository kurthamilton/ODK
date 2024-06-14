namespace ODK.Services.Recaptcha;

public class RecaptchaServiceSettings
{
    public double ScoreThreshold { get; set; }

    public string VerifyUrl { get; set; } = "";
}
