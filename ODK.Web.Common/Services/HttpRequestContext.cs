using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core.Utils;
using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public class HttpRequestContext : IHttpRequestContext
{
    public HttpRequestContext(HttpRequest? request)
    {
        RequestUrl = request?.GetDisplayUrl() ?? string.Empty;
        BaseUrl = UrlUtils.BaseUrl(RequestUrl);
    }

    public string BaseUrl { get; }

    public string RequestUrl { get; }
}