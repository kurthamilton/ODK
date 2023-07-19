using System;
using Newtonsoft.Json;

namespace ODK.Services.Payments.PayPal.Client
{
    public class OrderJsonModel
    {
        [JsonProperty("create_time")]
        public DateTime Created { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("purchase_units")]
        public PurchaseUnitJsonModel[] PurchaseUnits { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
