using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core.Utils;
using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public class HttpRequestContext : IHttpRequestContext
{
    private readonly Lazy<string> _baseUrl;

    public HttpRequestContext()
    {
        _baseUrl = new(() => UrlUtils.BaseUrl(RequestUrl ?? string.Empty));
    }

    public string BaseUrl => _baseUrl.Value;

    public required string RequestPath { get; init; }

    public required string RequestUrl { get; init; }

    public required string UserAgent { get; init; }

    public static HttpRequestContext Create(HttpRequest? request)
    {
        return new HttpRequestContext
        {
            RequestPath = request?.Path.Value ?? string.Empty,
            RequestUrl = request?.GetDisplayUrl() ?? string.Empty,
            UserAgent = request?.Headers.UserAgent.ToString() ?? string.Empty
        };
    }
}