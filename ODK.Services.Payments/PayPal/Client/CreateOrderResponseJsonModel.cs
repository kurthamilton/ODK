using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client;
public class CreateOrderResponseJsonModel
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }
}
