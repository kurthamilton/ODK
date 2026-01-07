namespace ODK.Services.Recaptcha;

public class RecaptchaResult
{
    public IReadOnlyCollection<string> Errors { get; init; } = [];

    public float Score { get; init; }

    public required bool Success { get; init; }
}
