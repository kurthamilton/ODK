using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class PricingSchemeJsonModel
{
    [JsonProperty("fixed_price")]
    public FixedPriceJsonModel? FixedPrice { get; set; }
}
