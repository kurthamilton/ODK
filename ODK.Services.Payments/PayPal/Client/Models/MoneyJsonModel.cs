using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class MoneyJsonModel
{
    [JsonProperty("currency_code")]
    public string CurrencyCode { get; set; } = "";

    [JsonProperty("value")]
    public decimal Value { get; set; }
}
