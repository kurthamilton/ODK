using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

// https://developer.paypal.com/docs/api/subscriptions/v1/#plans_create!path=billing_cycles&t=request
public class BillingCycleJsonModel
{
    [JsonProperty("frequency")]
    public FrequencyJsonModel Frequency { get; set; } = new();

    [JsonProperty("pricing_scheme")]
    public PricingSchemeJsonModel PricingScheme { get; set; } = new();

    [JsonProperty("sequence")]
    public int Sequence { get; set; }

    [JsonProperty("tenure_type")]
    public string TenureType { get; set; } = "";

    [JsonProperty("total_cycles")]
    public int TotalCycles { get; set; }
}
