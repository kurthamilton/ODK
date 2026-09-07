namespace ODK.Services.Integrations.Geolocation;

public class GeolocationServiceSettings
{
    public required string GoogleApiKey { get; init; }

    public bool GoogleDisabled { get; init; }

    public required string IpDatabaseDirectory { get; init; }

    public required bool PreloadIpDatabase { get; init; }
}
