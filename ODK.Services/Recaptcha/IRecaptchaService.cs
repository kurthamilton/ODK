namespace ODK.Services.Recaptcha;

public interface IRecaptchaService
{
    /// <summary>
    /// False when reCAPTCHA is switched off for the environment (see RecaptchaSettings.Enabled). Views use
    /// this to skip rendering the widget; verification short-circuits to a pass regardless.
    /// </summary>
    bool Enabled { get; }

    string GetSiteKey();

    bool Success(RecaptchaResult response);

    Task<RecaptchaResult> Verify(string token);
}