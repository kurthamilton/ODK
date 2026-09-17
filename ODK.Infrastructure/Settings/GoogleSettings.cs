namespace ODK.Infrastructure.Settings;

public class GoogleSettings
{
    public required Dictionary<PlatformKey, GooglePlatformSettings> Platforms { get; init; }
}
