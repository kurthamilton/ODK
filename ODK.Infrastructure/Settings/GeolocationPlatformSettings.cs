namespace ODK.Infrastructure.Settings;

public class GeolocationPlatformSettings
{
    /* False leaves the database unopened until a lookup asks for it. Worth turning off where the platform
       never asks: the open costs 20ms and the first lookup a further 14ms, so preloading only moves that
       off the first request. */
    public required bool PreloadIpDatabase { get; init; }
}
