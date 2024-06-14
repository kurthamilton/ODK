namespace ODK.Services.Recaptcha;

public interface IRecaptchaService
{
    bool Success(ReCaptchaResponse response);

    Task<ReCaptchaResponse> Verify(string token);
}
