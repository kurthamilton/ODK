using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchResponseHeaderJsonModel
{
    [JsonPropertyName("batch_status")]
    public string? BatchStatus { get; set; }

    [JsonPropertyName("payout_batch_id")]
    public string? PayoutBatchId { get; set; }
}
