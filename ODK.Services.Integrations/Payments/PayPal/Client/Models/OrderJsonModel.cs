using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class OrderJsonModel
{
    [JsonPropertyName("create_time")]
    public DateTime Created { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("purchase_units")]
    public PurchaseUnitJsonModel[] PurchaseUnits { get; set; } = Array.Empty<PurchaseUnitJsonModel>();

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}
