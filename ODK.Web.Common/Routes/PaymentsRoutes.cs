using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class PaymentsRoutes : RoutesBase
{
    public string Payments(PlatformType platform, Chapter chapter)
        => $"{Group(platform, chapter)}/payments";

    public string Group(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.GetDisplayName(platform).ToLowerInvariant()}/admin",
        _ => $"{Index(platform)}/{chapter.Id}"
    };

    public string Index(PlatformType platform) => platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => "/my/groups"
    };
}
