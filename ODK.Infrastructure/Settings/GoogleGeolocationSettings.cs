namespace ODK.Infrastructure.Settings;

public class GoogleGeolocationSettings
{
    public required bool Disabled { get; init; }

    /// <summary>
    /// Called from the server, so it never reaches a page and is restricted by address rather than by
    /// referrer. Distinct from the Maps client key for that reason, not by accident.
    /// </summary>
    public required string ServerApiKey { get; init; }
}
