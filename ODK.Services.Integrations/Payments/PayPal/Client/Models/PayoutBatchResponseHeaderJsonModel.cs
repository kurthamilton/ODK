using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchResponseHeaderJsonModel
{
    [JsonProperty("batch_status")]
    public string? BatchStatus { get; set; }

    [JsonProperty("payout_batch_id")]
    public string? PayoutBatchId { get; set; }
}
