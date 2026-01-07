using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class ProductResponseJsonModel : ProductJsonModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
}
