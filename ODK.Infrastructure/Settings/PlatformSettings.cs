namespace ODK.Infrastructure.Settings;

public class PlatformSettings
{
    public required string Name { get; init; }

    /// <summary>
    /// The platform's canonical base URL, for building a link to a site without a request to read one from.
    /// One URL rather than every host that reaches the platform: a request's own host says which platform it
    /// is for nowhere in the app, so an alternate host is a DNS and site-binding concern.
    /// </summary>
    public required string Url { get; init; }
}
