using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client;

public class CreateOrderJsonModel
{
    [JsonProperty("intent")]
    public required string Intent { get; init; }

    [JsonProperty("purchase_units")]
    public required PurchaseUnitJsonModel[] PurchaseUnits { get; init; }
}
