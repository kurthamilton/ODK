using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SiteAdmin;

public abstract class ChapterSiteAdminPageModel : AdminPageModel
{
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