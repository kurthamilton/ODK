using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public class HttpRequestContext : IHttpRequestContext
{
    public HttpRequestContext(HttpRequest? request)
    {
        RequestUrl = request?.GetDisplayUrl() ?? string.Empty;
    }

    public string RequestUrl { get; }
}
