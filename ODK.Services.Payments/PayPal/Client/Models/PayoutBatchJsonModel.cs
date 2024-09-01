using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class PayoutBatchJsonModel
{
    [JsonProperty("items")]
    public required IReadOnlyCollection<PayoutBatchItemJsonModel> Items { get; init; }

    [JsonProperty("sender_batch_header")]
    public required PayoutBatchSenderHeaderJsonModel SenderBatchHeader { get; init; }
}
