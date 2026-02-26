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

    public required string IpAddress { get; init; }

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
            IpAddress = request?.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?.Split(',')
                    .FirstOrDefault()
                    ?.Trim()
                ?? request?.HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? string.Empty,
            RequestPath = request?.Path.Value ?? string.Empty,
            RequestUrl = request?.GetDisplayUrl() ?? string.Empty,
            RouteValues = routeValues,
            UserAgent = request?.Headers.UserAgent.ToString() ?? string.Empty
        };
    }
}