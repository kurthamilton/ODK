using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class SubscriptionPlanJsonModel
{
    [JsonProperty("billing_cycles")]
    public BillingCycleJsonModel[] BillingCycles { get; set; } = [];

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("payment_preferences")]
    public PaymentPreferencesJsonModel PaymentPreferences { get; set; } = new();

    [JsonProperty("product_id")]
    public string ProductId { get; set; } = "";

    [JsonProperty("status")]
    public string Status { get; set; } = "";
}
