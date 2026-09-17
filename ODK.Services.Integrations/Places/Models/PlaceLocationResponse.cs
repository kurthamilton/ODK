using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Places.Models;

public class PlaceLocationResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }
}
