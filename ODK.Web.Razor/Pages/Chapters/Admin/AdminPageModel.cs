using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Security;
using ODK.Web.Common.Routes;
using MemberChapterAdminServiceRequestImpl = ODK.Services.MemberChapterAdminServiceRequest;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

public abstract class AdminPageModel : OdkPageModel
{
    private readonly Lazy<IMemberChapterAdminServiceRequest> _memberChapterAdminServiceRequest;

    protected AdminPageModel()
    {
        _memberChapterAdminServiceRequest =
            new(() => MemberChapterAdminServiceRequestImpl.Create(Securable, MemberChapterServiceRequest));
    }

    public GroupAdminRoutes AdminRoutes => OdkRoutes.GroupAdmin;

    public IMemberChapterAdminServiceRequest MemberChapterAdminServiceRequest => _memberChapterAdminServiceRequest.Value;

    public abstract ChapterAdminSecurable Securable { get; }

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (CurrentMemberOrDefault == null)
        {
            RedirectToLogin();
            return;
        }

        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        if (!adminMember.HasAccessTo(Securable, CurrentMember))
        {
            await RedirectToLanding();
            return;
        }

        await next();
    }

    public async Task Redirect(GroupAdminRoute route)
    {
        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        var permittedRoute = route.GetPermitted(adminMember, CurrentMember);
        if (permittedRoute != null)
        {
            Response.Redirect(permittedRoute.Path);
            return;
        }

        await RedirectToLanding();
    }

    protected void RedirectToLogin()
    {
        var returnUrl = $"{Request.Path}{Request.QueryString}";
        Response.Redirect(OdkRoutes.Account.Login(RequestStore.ChapterOrDefault, returnUrl));
    }

    /// <summary>
    /// Sends the member to an admin page they can actually open. A fixed fallback route cannot work
    /// here: a member who lacks access to that route is redirected to it, bounced again, and loops.
    /// </summary>
    private async Task RedirectToLanding()
    {
        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        var route = AdminRoutes.LandingRoute(Chapter, adminMember, CurrentMember);
        if (route == null)
        {
            throw new OdkNotAuthorizedException();
        }

        Response.Redirect(route.Path);
    }
}