namespace ODK.Infrastructure.Settings;

public class LoggingSettings
{
    public required LoggingIgnoreExceptionSettings[] IgnoreExceptions { get; init; }

    public required Dictionary<PlatformKey, LoggingPlatformSettings> Platforms { get; init; }
}
