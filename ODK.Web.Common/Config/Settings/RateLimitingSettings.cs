namespace ODK.Web.Common.Config.Settings;

public class RateLimitingSettings
{
    public required string[] BlockPatterns { get; init; }

    public required int BlockForSeconds { get; init; }
}
