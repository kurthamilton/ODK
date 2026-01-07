using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchJsonModel
{
    [JsonPropertyName("items")]
    public required IReadOnlyCollection<PayoutBatchItemJsonModel> Items { get; init; }

    [JsonPropertyName("sender_batch_header")]
    public required PayoutBatchSenderHeaderJsonModel SenderBatchHeader { get; init; }
}
