using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

public class UrlProvider : IUrlProvider
{
    public string EventsUrl(PlatformType platform, Chapter chapter) => OdkRoutes.Chapters.Events(platform, chapter);
}
