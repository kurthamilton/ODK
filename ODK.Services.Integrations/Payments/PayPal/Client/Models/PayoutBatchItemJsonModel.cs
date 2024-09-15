using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchItemJsonModel
{
    [JsonProperty("amount")]
    public required PayoutAmountJsonModel Amount { get; init; }

    [JsonProperty("note")]
    public string? Note { get; init; }

    [JsonProperty("receiver")]
    public required string Receiver { get; init; }

    [JsonProperty("recipient_type")]
    public required string RecipientType { get; init; }
}
