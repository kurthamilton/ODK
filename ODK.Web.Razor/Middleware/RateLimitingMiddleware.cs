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
    private static readonly ConcurrentDictionary<string, DateTime> GreyList = new(StringComparer.OrdinalIgnoreCase);

    // Expired grey-list entries are swept periodically so the dictionary can't grow unbounded.
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(10);
    private static long _nextCleanupTicks;

    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    // BlockPatterns come from the singleton AppSettings and don't change at runtime, so the pattern is
    // compiled once here rather than on every request.
    private readonly Regex? _blockPatternRegex;

    public RateLimitingMiddleware(RequestDelegate next, AppSettings appSettings)
    {
        _next = next;
        _appSettings = appSettings;

        var blockPatterns = appSettings.RateLimiting.BlockPatterns;
        _blockPatternRegex = blockPatterns.Length > 0
            ? new Regex($@"^({string.Join('|', blockPatterns)})$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            : null;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ILoggingService loggingService)
    {
        var utcNow = DateTime.UtcNow;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        // Check black list - block immediately, don't run the rest of the pipeline.
        if (_appSettings.RateLimiting.BlockIpAddresses.Contains(ipAddress, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        CleanupExpired(utcNow);

        // Get previous rate limit
        var blockedUntilUtc = GreyList.TryGetValue(ipAddress, out var value)
            ? value
            : default(DateTime?);

        // Does the current request warrant rate limiting?
        var path = UrlUtils.NormalisePath(context.Request.Path);
        var isRateLimited = IsRateLimited(path);

        var shouldBlock = isRateLimited || blockedUntilUtc > utcNow;
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

        if (blockedUntilUtc == null || blockedUntilUtc < utcNow)
        {
            blockedUntilUtc = utcNow;
        }

        blockedUntilUtc = blockedUntilUtc.Value.AddSeconds(_appSettings.RateLimiting.BlockForSeconds);

        GreyList[ipAddress] = blockedUntilUtc.Value;
    }

    private bool IsRateLimited(string requestPath)
    {
        if (_appSettings.RateLimiting.BlockPaths.Any(x => MatchesConfigRule(x, requestPath)))
        {
            return true;
        }

        return _blockPatternRegex?.IsMatch(requestPath) == true;
    }

    private static void CleanupExpired(DateTime utcNow)
    {
        var next = Interlocked.Read(ref _nextCleanupTicks);
        if (utcNow.Ticks < next)
        {
            return;
        }

        // Claim the next cleanup window; if another thread got there first, let it do the work.
        var newNext = utcNow.Add(CleanupInterval).Ticks;
        if (Interlocked.CompareExchange(ref _nextCleanupTicks, newNext, next) != next)
        {
            return;
        }

        foreach (var entry in GreyList)
        {
            if (entry.Value <= utcNow)
            {
                GreyList.TryRemove(entry.Key, out _);
            }
        }
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
