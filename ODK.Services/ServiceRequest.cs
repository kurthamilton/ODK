using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class ServiceRequest
{
    public ServiceRequest(
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
    {
        HttpRequestContext = httpRequestContext;
        Platform = platform;
    }

    public IHttpRequestContext HttpRequestContext { get; }

    public PlatformType Platform { get; }
}
