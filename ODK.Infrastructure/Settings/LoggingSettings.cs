using ODK.Services.Logging;

namespace ODK.Infrastructure.Settings;

public class LoggingSettings
{
    public IgnoreExceptionRule[] IgnoreExceptions { get; init; } = [];

    public required string Path { get; init; }
}