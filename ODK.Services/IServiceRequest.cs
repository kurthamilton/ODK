using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public interface IServiceRequest
{
    Guid? CurrentMemberIdOrDefault { get; }

    IHttpRequestContext HttpRequestContext { get; }

    PlatformType Platform { get; }
}