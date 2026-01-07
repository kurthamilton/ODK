using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class PurchaseUnitJsonModel
{
    [JsonPropertyName("amount")]
    public MoneyJsonModel? Amount { get; set; }

    [JsonPropertyName("reference_id")]
    public string? ReferenceId { get; set; }
}
