using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PricingSchemeJsonModel
{
    [JsonPropertyName("fixed_price")]
    public FixedPriceJsonModel? FixedPrice { get; set; }
}
