namespace ODK.Services.Recaptcha;

public interface IRecaptchaService
{
    bool Success(RecaptchaResult response);

    Task<RecaptchaResult> Verify(string token);
}
