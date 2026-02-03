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
        var member = CurrentMember;
        if (member.SiteAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }

        await next();
    }
}