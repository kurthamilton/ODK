namespace ODK.Web.Common.Settings;

/// <summary>
/// What <c>RateLimitingMiddleware</c> needs from <c>RateLimiting</c> configuration, mapped in
/// <c>DependencyRegistrar</c>.
/// </summary>
public class RateLimitingMiddlewareSettings
{
    public required int BlockForSeconds { get; init; }

    public required string[] BlockIpAddresses { get; init; }

    public required string[] BlockPaths { get; init; }

    public required string[] BlockPatterns { get; init; }
}
