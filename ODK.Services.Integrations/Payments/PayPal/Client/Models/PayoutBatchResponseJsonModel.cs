using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchResponseJsonModel
{
    [JsonPropertyName("batch_header")]
    public PayoutBatchResponseHeaderJsonModel? BatchHeader { get; set; }
}
