using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class FixedPriceJsonModel
{
    [JsonProperty("currency_code")]
    public required string CurrencyCode { get; init; }

    [JsonProperty("value")]
    public required string Value { get; init; }
}
