using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core.Utils;
using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public class HttpRequestContext : IHttpRequestContext
{
    private readonly Lazy<string> _baseUrl;

    public HttpRequestContext(HttpRequest? request)
    {
        RequestUrl = request?.GetDisplayUrl() ?? string.Empty;

        _baseUrl = new(() => UrlUtils.BaseUrl(RequestUrl));
    }

    public string BaseUrl => _baseUrl.Value;

    public string RequestUrl { get; }
}
