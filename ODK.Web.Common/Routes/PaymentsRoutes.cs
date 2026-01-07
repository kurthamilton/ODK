using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class PaymentsRoutes : RoutesBase
{
    public string PaymentAccount(PlatformType platform, Chapter chapter)
        => $"{Payments(platform, chapter)}/account";

    public string Payments(PlatformType platform, Chapter chapter)
        => $"{Group(platform, chapter)}/payments";

    private static string Group(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.GetDisplayName(platform).ToLowerInvariant()}/admin",
        _ => $"{Index(platform)}/{chapter.Id}"
    };

    private static string Index(PlatformType platform) => platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => "/my/groups"
    };
}
