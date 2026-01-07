using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Recaptcha.Models;

public class ReCaptchaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("challenge_ts")]
    public DateTime ChallengeTs { get; set; }

    [JsonPropertyName("hostname")]
    public string? HostName { get; set; }

    [JsonPropertyName("error-codes")]
    public string[] ErrorCodes { get; set; } = Array.Empty<string>();
}
