using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Exceptions;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SiteAdmin;

public abstract class ChapterSiteAdminPageModel : AdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Any;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        // Overrides the base handler (site-admin, not chapter-admin), so it repeats the unauthenticated
        // guard rather than letting CurrentMember throw on an expired session.
        var member = CurrentMemberOrDefault;
        if (member == null)
        {
            RedirectToLogin();
            return;
        }

        if (member.SiteAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }

        await next();
    }
}