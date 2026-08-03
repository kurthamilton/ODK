namespace ODK.Services.Logging;

public class LoggingServiceSettings
{
    public required IReadOnlyCollection<IgnoreExceptionRule> IgnoreExceptions { get; init; } = [];
}