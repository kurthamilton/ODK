using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class BetterStackSettings
{
    public required Dictionary<PlatformType, BetterStackPlatformSettings> Platforms { get; init; }
}
