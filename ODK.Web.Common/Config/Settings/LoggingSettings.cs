namespace ODK.Web.Common.Config.Settings;

public class LoggingSettings
{
    public required LoggingStatusCodeSettings NotFound { get; init; }

    public required string Path { get; init; }
}
