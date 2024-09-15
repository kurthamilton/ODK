using Newtonsoft.Json;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class ProductJsonModel
{
    [JsonProperty("category")]
    public string Category { get; set; } = "SOFTWARE";

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("type")]
    public string Type { get; set; } = "SERVICE";
}
