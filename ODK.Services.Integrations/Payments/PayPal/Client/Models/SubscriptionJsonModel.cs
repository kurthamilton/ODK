using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class SubscriptionJsonModel
{
    [JsonProperty("billing_info")]
    public BillingInfoJsonModel? BillingInfo { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("plan_id")]
    public string PlanId { get; set; } = "";

    [JsonProperty("status")]
    public string Status { get; set; } = "";
}
