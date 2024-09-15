using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PayoutBatchResponseJsonModel
{
    [JsonProperty("batch_header")]
    public PayoutBatchResponseHeaderJsonModel? BatchHeader { get; set; }
}
