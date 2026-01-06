using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Geolocation.Models;

public class GeocodeResponse
{
    [JsonPropertyName("results")]
    public GeocodeResponseResult[]? Results { get; init; }
}
