using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class PayoutAmountJsonModel
{
    [JsonProperty("currency")]
    public required string CurrencyCode { get; init; }

    [JsonProperty("value")]
    public required string Value { get; init; }
}
