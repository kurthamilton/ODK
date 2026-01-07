using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class SubscriptionPlanResponseJsonModel : SubscriptionPlanJsonModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
