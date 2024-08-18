using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class LastPaymentJsonModel
{
    [JsonProperty("time")]
    public DateTime? Date { get; set; }
}
