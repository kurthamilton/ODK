using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchItemJsonModel
{
    [JsonPropertyName("amount")]
    public required PayoutAmountJsonModel Amount { get; init; }

    [JsonPropertyName("note")]
    public string? Note { get; init; }

    [JsonPropertyName("receiver")]
    public required string Receiver { get; init; }

    [JsonPropertyName("recipient_type")]
    public required string RecipientType { get; init; }
}
