using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public abstract class SuperAdminPageModel : OdkPageModel
{
    private readonly IRequestCache _requestCache;

    protected SuperAdminPageModel(IRequestCache requestCache)
    {
        _requestCache = requestCache;
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        var member = await _requestCache.GetMemberAsync(CurrentMemberId);
        if (member?.SuperAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }
    }
}
