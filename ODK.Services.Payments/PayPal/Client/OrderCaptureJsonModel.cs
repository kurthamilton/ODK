using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client
{
    public class OrderCaptureJsonModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
