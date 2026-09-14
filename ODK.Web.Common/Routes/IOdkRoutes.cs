using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Common.Routes;

public interface IOdkRoutes
{
    AccountRoutes Account { get; }
    GroupRoutes Groups { get; }
    GroupAdminRoutes GroupAdmin { get; }
    MemberRoutes Members { get; }
    PaymentRoutes Payments { get; }
    PublicRoutes Public { get; }
    SiteRoutes Site { get; }
    SiteAdminRoutes SiteAdmin { get; }

    string Error(Chapter? chapter, int statusCode);

    /// <summary>
    /// Where a member goes once a sign-up that came here to do something in particular is complete. Null
    /// where the sign-up stated nothing, which leaves the caller's own destination.
    /// </summary>
    string? SignUpDestination(SignUpIntentType? intent);
}
