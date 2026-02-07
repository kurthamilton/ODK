using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Exceptions;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Groups;

public abstract class OdkGroupPageModel : OdkPageModel, IAsyncPageFilter
{
    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!Chapter.IsPublished())
        {
            var adminMember = await RequestStore.GetCurrentChapterAdminMember();
            if (!adminMember.HasAccessTo(ChapterAdminSecurable.Any, CurrentMember))
            {
                throw new OdkNotFoundException("Group not published");
            }
        }

        await next();
    }
}