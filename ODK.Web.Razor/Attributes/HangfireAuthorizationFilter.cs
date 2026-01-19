using Hangfire.Dashboard;
using ODK.Services.Authentication;

namespace ODK.Web.Razor.Attributes;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.IsInRole(OdkRoles.SiteAdmin);
    }
}
