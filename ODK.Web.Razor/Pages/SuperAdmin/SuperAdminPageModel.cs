using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public abstract class SuperAdminPageModel : OdkPageModel
{
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        var member = await RequestStore.GetCurrentMember();
        if (member?.SuperAdmin != true)
        {
            throw new OdkNotAuthorizedException();
        }
    }
}