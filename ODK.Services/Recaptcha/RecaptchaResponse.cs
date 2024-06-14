using Newtonsoft.Json;

namespace ODK.Services.Recaptcha;

public class ReCaptchaResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("score")]
    public float Score { get; set; }

    [JsonProperty("action")]
    public string? Action { get; set; }

    [JsonProperty("challenge_ts")]
    public DateTime ChallengeTs { get; set; }

    [JsonProperty("hostname")]
    public string? HostName { get; set; }

    [JsonProperty("error-codes")]
    public string[] ErrorCodes { get; set; } = Array.Empty<string>();
}
