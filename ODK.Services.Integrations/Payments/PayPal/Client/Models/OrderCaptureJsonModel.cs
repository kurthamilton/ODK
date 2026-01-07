using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class OrderCaptureJsonModel
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
