using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class PayoutBatchSenderHeaderJsonModel
{
    [JsonProperty("sender_batch_id")]
    public required string SenderBatchId { get; init; }
}
