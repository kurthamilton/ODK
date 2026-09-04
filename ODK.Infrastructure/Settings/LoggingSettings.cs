using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class LoggingSettings
{
    public required LoggingIgnoreExceptionSettings[] IgnoreExceptions { get; init; }

    public required Dictionary<PlatformType, LoggingPlatformSettings> Platforms { get; init; }
}
