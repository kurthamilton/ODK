using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Places.Models;

public class PlaceResponse
{
    [JsonPropertyName("addressComponents")]
    public PlaceAddressComponentResponse[]? AddressComponents { get; init; }

    [JsonPropertyName("displayName")]
    public PlaceLocalizedTextResponse? DisplayName { get; init; }

    [JsonPropertyName("formattedAddress")]
    public string? FormattedAddress { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("location")]
    public PlaceLocationResponse? Location { get; init; }
}
