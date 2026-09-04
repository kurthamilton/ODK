namespace ODK.Infrastructure.Settings;

public class LoggingPlatformSettings
{
    /// <summary>
    /// The directory this platform's instance writes its log files to. One per platform because Serilog holds
    /// the file it opens, so two instances pointed at one directory leaves one of them without a log.
    /// </summary>
    public required string Path { get; init; }
}
