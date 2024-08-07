using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Web.Razor.Pages.Chapters.Admin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public abstract class ChapterSuperAdminPageModel : AdminPageModel
{
    private readonly IRequestCache _requestCache;

    protected ChapterSuperAdminPageModel(IRequestCache requestCache)
        : base(requestCache)
    {
        _requestCache = requestCache;
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var member = await _requestCache.GetMemberAsync(CurrentMemberId);
        if (member?.SuperAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
