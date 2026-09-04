namespace ODK.Infrastructure.Settings;

public class BetterStackSettings
{
    public required Dictionary<PlatformKey, BetterStackPlatformSettings> Platforms { get; init; }
}
