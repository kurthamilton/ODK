namespace ODK.Infrastructure.Settings;

public class LoggingSettings
{
    public required LoggingIgnoreExceptionSettings[] IgnoreExceptions { get; init; }

    public required string Path { get; init; }
}
