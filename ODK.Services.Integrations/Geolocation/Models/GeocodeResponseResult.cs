using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Geolocation.Models;

public class GeocodeResponseResult
{
    [JsonPropertyName("address_components")]
    public AddressComponent[]? AddressComponents { get; init; }
}
