using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Geolocation.Models;

public class IpApiResponse
{
    [JsonPropertyName("city")]
    public string? City { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; init; }

    [JsonPropertyName("lat")]
    public double? Latitude { get; init; }

    [JsonPropertyName("lon")]
    public double? Longitude { get; init; }
}