namespace ODK.Services.Recaptcha;

public interface IRecaptchaService
{
    string GetSiteKey();

    bool Success(RecaptchaResult response);

    Task<RecaptchaResult> Verify(string token);
}