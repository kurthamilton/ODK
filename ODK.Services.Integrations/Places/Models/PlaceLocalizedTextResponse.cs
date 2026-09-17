using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Places.Models;

public class PlaceLocalizedTextResponse
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
