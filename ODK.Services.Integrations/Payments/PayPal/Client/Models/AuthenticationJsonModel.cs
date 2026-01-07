using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class AuthenticationJsonModel
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("app_id")]
    public string AppId { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}
