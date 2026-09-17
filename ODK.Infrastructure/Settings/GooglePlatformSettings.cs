namespace ODK.Infrastructure.Settings;

/// <summary>
/// One platform's Google keys. Keyed by platform because a key is restricted to the referrers or addresses
/// of the site that uses it, so two sites cannot share one however alike their usage is.
/// </summary>
public class GooglePlatformSettings
{
    public required GoogleGeolocationSettings Geolocation { get; init; }

    public required GoogleMapsSettings Maps { get; init; }

    public required GoogleOAuthSettings OAuth { get; init; }

    public required GooglePlacesSettings Places { get; init; }
}
