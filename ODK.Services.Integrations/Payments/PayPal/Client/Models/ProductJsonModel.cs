using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class ProductJsonModel
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "SOFTWARE";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "SERVICE";
}
