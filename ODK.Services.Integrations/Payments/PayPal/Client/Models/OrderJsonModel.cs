using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class OrderJsonModel
{
    [JsonProperty("create_time")]
    public DateTime Created { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("purchase_units")]
    public PurchaseUnitJsonModel[] PurchaseUnits { get; set; } = Array.Empty<PurchaseUnitJsonModel>();

    [JsonProperty("status")]
    public string Status { get; set; } = "";
}
