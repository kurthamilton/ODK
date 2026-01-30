using ODK.Core.Chapters;

namespace ODK.Web.Common.Routes;

public interface IOdkRoutes
{
    AccountRoutes Account { get; }
    GroupRoutes Groups { get; }
    GroupAdminRoutes GroupAdmin { get; }
    MemberRoutes Members { get; }
    PaymentsRoutes Payments { get; }
    SiteAdminRoutes SiteAdmin { get; }

    string Error(Chapter? chapter, int statusCode);
}
