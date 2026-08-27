namespace ODK.Services.Integrations.Geolocation;

public class GeolocationServiceSettings
{
    public required string GoogleApiKey { get; init; }

    // Set true in the E2E environment so sign-up/group creation don't hit the billable, rate-limited
    // external APIs (Google geocode, ip-api). When true the lat/long-based lookups return no result.
    // Defaults to false, so production stays enabled without any configuration.
    public bool GoogleDisabled { get; init; }
}
