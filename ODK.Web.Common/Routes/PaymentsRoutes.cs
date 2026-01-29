using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class PaymentsRoutes : RoutesBase
{
    public PaymentsRoutes(PlatformType platform) 
        : base(platform)
    {
    }

    public string PaymentAccount(Chapter chapter)
        => $"{Payments(chapter)}/account";

    public string Payments(Chapter chapter)
        => $"{Group(chapter)}/payments";

    private string Group(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.ShortName.ToLowerInvariant()}/admin",
        _ => $"{Index()}/{chapter.Id}"
    };

    private string Index() => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => "/my/groups"
    };
}
