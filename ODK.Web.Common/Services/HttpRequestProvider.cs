using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public class HttpRequestProvider : IHttpRequestProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string RequestUrl => _httpContextAccessor.HttpContext?.Request.GetDisplayUrl() ?? "";
}
