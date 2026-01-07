using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

// https://developer.paypal.com/docs/api/subscriptions/v1/#plans_create!path=billing_cycles&t=request
public class BillingCycleJsonModel
{
    [JsonPropertyName("frequency")]
    public FrequencyJsonModel Frequency { get; set; } = new();

    [JsonPropertyName("pricing_scheme")]
    public PricingSchemeJsonModel PricingScheme { get; set; } = new();

    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonPropertyName("tenure_type")]
    public string TenureType { get; set; } = "";

    [JsonPropertyName("total_cycles")]
    public int TotalCycles { get; set; }
}
