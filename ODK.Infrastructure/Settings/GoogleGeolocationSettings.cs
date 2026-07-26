namespace ODK.Infrastructure.Settings;

public class GoogleGeolocationSettings
{
    public required string ApiKey { get; init; }

    public bool Disabled { get; init; }
}
