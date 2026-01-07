using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchSenderHeaderJsonModel
{
    [JsonPropertyName("sender_batch_id")]
    public required string SenderBatchId { get; init; }
}
