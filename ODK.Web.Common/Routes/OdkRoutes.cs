using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public static class OdkRoutes
{
    public static AccountRoutes Account { get; } = new();
    public static GroupRoutes Groups { get; } = new();
    public static MemberGroupRoutes MemberGroups { get; } = new();
    public static MemberRoutes Members { get; } = new();
    public static PaymentsRoutes Payments { get; } = new();
    public static SiteAdminRoutes SiteAdmin { get; } = new();

    public static string Error(PlatformType platform, Chapter? chapter, int statusCode)
        => chapter != null
            ? Groups.Error(platform, chapter, statusCode)
            : $"/error/{statusCode}";
}