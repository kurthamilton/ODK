using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class SubscriptionJsonModel
{
    [JsonPropertyName("billing_info")]
    public BillingInfoJsonModel? BillingInfo { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("plan_id")]
    public string PlanId { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}
