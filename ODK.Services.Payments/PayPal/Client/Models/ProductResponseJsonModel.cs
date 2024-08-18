using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class ProductResponseJsonModel : ProductJsonModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";
}
