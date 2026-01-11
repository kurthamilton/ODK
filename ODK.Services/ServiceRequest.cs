using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class ServiceRequest
{
    public ServiceRequest()
    {
    }

    public required IHttpRequestContext HttpRequestContext { get; init; }

    public required PlatformType Platform { get; init; }
}