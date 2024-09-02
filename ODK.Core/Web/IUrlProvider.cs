using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Core.Web;

public interface IUrlProvider
{
    string EventsUrl(PlatformType platform, Chapter chapter);

    string MessageAdminUrl(PlatformType platform, Guid messageId);

    string MessageAdminUrl(PlatformType platform, Chapter chapter, Guid messageId);
}
