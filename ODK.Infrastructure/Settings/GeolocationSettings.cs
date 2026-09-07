namespace ODK.Infrastructure.Settings;

public class GeolocationSettings
{
    /* Absolute, and outside the application directory: the scheduled update writes here between deploys,
       so a path under the app would be replaced by the next publish. */
    public required string IpDatabaseDirectory { get; init; }

    public required Dictionary<PlatformKey, GeolocationPlatformSettings> Platforms { get; init; }
}
