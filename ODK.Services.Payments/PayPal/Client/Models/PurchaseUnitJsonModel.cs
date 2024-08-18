using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client.Models;

public class PurchaseUnitJsonModel
{
    [JsonProperty("amount")]
    public MoneyJsonModel? Amount { get; set; }

    [JsonProperty("reference_id")]
    public string? ReferenceId { get; set; }
}
