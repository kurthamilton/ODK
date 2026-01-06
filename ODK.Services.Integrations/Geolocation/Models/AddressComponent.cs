using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Geolocation.Models;

public class AddressComponent
{
    [JsonPropertyName("long_name")]
    public string? LongName { get; init; }

    [JsonPropertyName("short_name")]
    public string? ShortName { get; init; }

    [JsonPropertyName("types")]
    public string[]? Types { get; init; }
}
