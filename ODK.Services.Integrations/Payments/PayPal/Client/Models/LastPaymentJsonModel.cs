using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class LastPaymentJsonModel
{
    [JsonPropertyName("time")]
    public DateTime? Date { get; set; }
}
