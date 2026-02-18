namespace ODK.Infrastructure.Settings;

public class RateLimitingSettings
{
    public required string[] BlockIpAddresses { get; init; }

    public required string[] BlockPaths { get; init; }

    public required string[] BlockPatterns { get; init; }

    public required int BlockForSeconds { get; init; }
}