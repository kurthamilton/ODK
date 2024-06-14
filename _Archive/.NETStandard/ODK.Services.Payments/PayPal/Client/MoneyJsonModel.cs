using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client
{
    public class MoneyJsonModel
    {
        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; } = "";

        [JsonProperty("value")]
        public double Value { get; set; }
    }
}
