using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public interface IOdkRoutesFactory
{
    /// <summary>
    /// The route tree for a platform. Takes the platform rather than reading the served one, because a URL
    /// about a group is built against the platform that owns it - see <see cref="Services.UrlProvider"/> -
    /// so one scope routinely asks for both.
    /// </summary>
    IOdkRoutes Get(PlatformType platform);
}
