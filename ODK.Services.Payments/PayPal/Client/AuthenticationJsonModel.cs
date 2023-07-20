using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client
{
    public class AuthenticationJsonModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("app_id")]
        public string AppId { get; set; } = "";

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; } = "";

        [JsonProperty("scope")]
        public string Scope { get; set; } = "";

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "";
    }
}
