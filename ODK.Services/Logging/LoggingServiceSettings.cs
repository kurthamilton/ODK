namespace ODK.Services.Logging;

public class LoggingServiceSettings
{
    public required IReadOnlyCollection<string> IgnoreUnknownPathPatterns { get; init; }

    public required IReadOnlyCollection<string> IgnoreUnknownPaths { get; init; }

    public required IReadOnlyCollection<string> IgnoreUnknownPathUserAgents { get; init; }
}