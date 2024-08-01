using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Web.Razor.Pages.Chapters.Admin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public abstract class SuperAdminPageModel : AdminPageModel
{
    protected SuperAdminPageModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var member = await RequestCache.GetMemberAsync(CurrentMemberId);
        if (member?.SuperAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
