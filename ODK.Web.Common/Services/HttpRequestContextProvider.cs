using Microsoft.AspNetCore.Http;
using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public class HttpRequestContextProvider : IHttpRequestContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IHttpRequestContext Get()
        => new HttpRequestContext(_httpContextAccessor.HttpContext?.Request);
}
