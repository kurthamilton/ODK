using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using ODK.Core.Utils;
using ODK.Infrastructure.Settings;
using ODK.Services.Logging;

namespace ODK.Web.Razor.Middleware;

public class RateLimitingMiddleware
{
    // Collection of IP addresses and the time they are blocked until
    private static readonly ConcurrentDictionary<string, DateTime> GreyList = new(StringComparer.InvariantCultureIgnoreCase);

    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ILoggingService loggingService,
        AppSettings appSettings)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        // Check black list
        if (appSettings.RateLimiting.BlockIpAddresses.Contains(ipAddress, StringComparer.OrdinalIgnoreCase))
        {
            // Use 403 for black-listed IP addresses
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }

        // Get previous rate limit
        var blockedUntilUtc = GreyList.TryGetValue(ipAddress, out var value)
            ? value
            : default(DateTime?);

        // Does the current request warrant rate limiting?
        var path = UrlUtils.NormalisePath(context.Request.Path);
        var isRateLimited = IsRateLimited(path, appSettings.RateLimiting);

        var shouldBlock = isRateLimited || blockedUntilUtc > DateTime.UtcNow;
        if (!shouldBlock)
        {
            // No reason to block - run the remaining pipeline
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            await loggingService.Warn($"Authenticated user has triggered rate limiting middleware");
        }

        // Use 429 for lack of more accurate status code
        // The first instance of a grey-listed request will trigger a rate limit
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;

        if (!isRateLimited)
        {
            // The current request doesn't warrant rate limiting - do not increase the block time
            return;
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            // Simply block the request if the IP address is not provided
            await loggingService.Warn($"Rate-limiting: ip address not provided");
            return;
        }

        if (blockedUntilUtc == null || blockedUntilUtc < DateTime.UtcNow)
        {
            blockedUntilUtc = DateTime.UtcNow;
        }

        blockedUntilUtc = blockedUntilUtc.Value.AddSeconds(appSettings.RateLimiting.BlockForSeconds);

        GreyList[ipAddress] = blockedUntilUtc.Value;
    }

    private static bool IsRateLimited(string requestPath, RateLimitingSettings settings)
    {
        if (settings.BlockPatterns.Length == 0 && settings.BlockPaths.Length == 0)
        {
            return false;
        }

        if (settings.BlockPaths.Any(x => MatchesConfigRule(x, requestPath)))
        {
            return true;
        }

        var pattern = $@"^({string.Join('|', settings.BlockPatterns)})$";
        return Regex.IsMatch(requestPath, pattern, RegexOptions.IgnoreCase);
    }

    private static bool MatchesConfigRule(string rule, string value)
    {
        var (wildStart, wildEnd) = (rule.StartsWith('*'), rule.EndsWith('*'));

        if (wildStart && wildEnd)
        {
            return value.Contains(rule[1..^1], StringComparison.OrdinalIgnoreCase);
        }

        if (wildStart)
        {
            return value.EndsWith(rule[1..], StringComparison.OrdinalIgnoreCase);
        }

        if (wildEnd)
        {
            return value.StartsWith(rule[..^1], StringComparison.OrdinalIgnoreCase);
        }

        return value.Equals(rule, StringComparison.OrdinalIgnoreCase);
    }
}