using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core.Utils;
using ODK.Core.Web;

namespace ODK.Web.Razor.Services;

public class HttpRequestContext : IHttpRequestContext
{
    private readonly Lazy<string> _baseUrl;

    public HttpRequestContext()
    {
        _baseUrl = new(() => UrlUtils.BaseUrl(RequestUrl ?? string.Empty));
    }

    public string BaseUrl => _baseUrl.Value;

    public required IReadOnlyDictionary<string, string[]> Headers { get; init; }

    public required string IpAddress { get; init; }

    public required string? Locale { get; init; }

    public required string RequestPath { get; init; }

    public required string RequestUrl { get; init; }

    public required IReadOnlyDictionary<string, string?> RouteValues { get; init; }

    public required string UserAgent { get; init; }

    public static HttpRequestContext Create(HttpRequest? request)
    {
        var routeValues = new Dictionary<string, string?>();

        if (request != null)
        {
            foreach (var routeValue in request.RouteValues)
            {
                routeValues[routeValue.Key] = routeValue.Value?.ToString();
            }
        }

        return new HttpRequestContext
        {
            Headers = GetHeaders(request),
            IpAddress = request?.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?.Split(',')
                    .FirstOrDefault()
                    ?.Trim()
                ?? request?.HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? string.Empty,
            Locale = LocaleUtils.GetPreferredLocale(GetAcceptLanguages(request)),
            RequestPath = request?.Path.Value ?? string.Empty,
            RequestUrl = request?.GetDisplayUrl() ?? string.Empty,
            RouteValues = routeValues,
            UserAgent = request?.Headers.UserAgent.ToString() ?? string.Empty
        };
    }

    /* Keyed case-insensitively, as the request's own header collection is, so a caller that does look a header
       up by name gets the same answer ASP.NET would give. A null value becomes empty rather than being dropped:
       a header that arrived with no value is still a header that arrived. */
    private static IReadOnlyDictionary<string, string[]> GetHeaders(HttpRequest? request)
        => request?.Headers.ToDictionary(
                x => x.Key,
                x => x.Value.Select(value => value ?? string.Empty).ToArray(),
                StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    // Accept-Language values, highest quality first (an omitted quality defaults to 1.0 - highest).
    private static IEnumerable<string?> GetAcceptLanguages(HttpRequest? request)
        => request?.GetTypedHeaders().AcceptLanguage
            .OrderByDescending(x => x.Quality ?? 1)
            .Select(x => x.Value.Value)
        ?? [];
}