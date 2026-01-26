namespace ODK.Web.Common.Config.Settings;

public class LoggingStatusCodeSettings
{
    public required string[] IgnorePaths { get; init; }

    public required string[] IgnorePatterns { get; init; }

    public required string[] WarningUserAgents { get; init; }
}