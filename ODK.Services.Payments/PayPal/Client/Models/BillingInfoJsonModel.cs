using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class BillingInfoJsonModel
{
    [JsonProperty("last_payment")]
    public LastPaymentJsonModel? LastPayment { get; set; }

    [JsonProperty("next_billing_time")]
    public DateTime? NextBillingDate { get; set; }
}
