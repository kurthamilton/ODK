using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class FixedPriceJsonModel
{
    [JsonPropertyName("currency_code")]
    public required string CurrencyCode { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}
