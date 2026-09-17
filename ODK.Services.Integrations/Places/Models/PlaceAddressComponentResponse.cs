using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Places.Models;

/* Not Geolocation.Models.AddressComponent: that one is the Geocoding API's shape, which names the same
   things long_name and short_name. */
public class PlaceAddressComponentResponse
{
    [JsonPropertyName("longText")]
    public string? LongText { get; init; }

    [JsonPropertyName("shortText")]
    public string? ShortText { get; init; }

    [JsonPropertyName("types")]
    public string[]? Types { get; init; }
}
