namespace ODK.Infrastructure.Settings;

public class LoggingSettings
{
    public required string[] IgnorePaths { get; init; }

    public required string[] IgnorePatterns { get; init; }

    public required string[] IgnoreUserAgents { get; init; }

    public required string Path { get; init; }
}