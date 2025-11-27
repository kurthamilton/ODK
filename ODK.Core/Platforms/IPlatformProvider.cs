using ODK.Core.Web;

namespace ODK.Core.Platforms;

public interface IPlatformProvider
{
    PlatformType GetPlatform();

    PlatformType GetPlatform(string requestUrl);

    PlatformType GetPlatform(IHttpRequestContext httpRequestContext);
}
