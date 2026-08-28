using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Exceptions;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.SiteAdmin;

/// <summary>
/// Base class for the /my/groups/{chapterId}/siteadmin/* pages. These sit inside the group admin tree
/// but are gated on site admin rather than on a group role, so the securable only names a role the
/// service request can be built with.
/// </summary>
public abstract class GroupSiteAdminPageModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Any;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!CurrentMember.SiteAdmin)
        {
            throw new OdkNotAuthorizedException();
        }

        await next();
    }
}
