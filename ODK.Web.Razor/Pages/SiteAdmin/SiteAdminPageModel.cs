using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Pages.SiteAdmin;

[Authorize]
public abstract class SiteAdminPageModel : OdkPageModel
{
    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var member = await RequestStore.GetCurrentMember();
        if (member?.SiteAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }

        await next();
    }
}