using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class BillingInfoJsonModel
{
    [JsonPropertyName("last_payment")]
    public LastPaymentJsonModel? LastPayment { get; set; }

    [JsonPropertyName("next_billing_time")]
    public DateTime? NextBillingDate { get; set; }
}
