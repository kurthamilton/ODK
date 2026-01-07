using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class SubscriptionPlanJsonModel
{
    [JsonPropertyName("billing_cycles")]
    public BillingCycleJsonModel[] BillingCycles { get; set; } = [];

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("payment_preferences")]
    public PaymentPreferencesJsonModel PaymentPreferences { get; set; } = new();

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
