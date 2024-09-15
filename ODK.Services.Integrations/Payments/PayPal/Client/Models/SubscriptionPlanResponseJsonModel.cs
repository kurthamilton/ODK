using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class SubscriptionPlanResponseJsonModel : SubscriptionPlanJsonModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";
}
