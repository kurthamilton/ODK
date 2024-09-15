using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class OrderCaptureJsonModel
{
    [JsonProperty("status")]
    public string Status { get; set; } = "";
}
