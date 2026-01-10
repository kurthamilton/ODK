using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public abstract class ChapterSuperAdminPageModel : AdminPageModel
{
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var member = await RequestStore.GetCurrentMember();
        if (member?.SuperAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}