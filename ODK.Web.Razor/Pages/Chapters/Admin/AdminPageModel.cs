using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services;
using ODK.Services.Security;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

public abstract class AdminPageModel : OdkPageModel
{
    private readonly Lazy<MemberChapterAdminServiceRequest> _memberChapterAdminServiceRequest;

    protected AdminPageModel()
    {
        _memberChapterAdminServiceRequest =
            new(() => MemberChapterAdminServiceRequest.Create(Securable, MemberChapterServiceRequest));
    }

    public GroupAdminRoutes AdminRoutes => OdkRoutes.GroupAdmin;

    public MemberChapterAdminServiceRequest MemberChapterAdminServiceRequest => _memberChapterAdminServiceRequest.Value;

    public abstract ChapterAdminSecurable Securable { get; }    

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        if (!adminMember.HasAccessTo(Securable, CurrentMember))
        {
            await Redirect(AdminRoutes.Events(Chapter));
            return;
        }

        await next();
    }

    public async Task Redirect(GroupAdminRoute route)
    {
        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        var permittedRoute = 
            route.GetPermitted(adminMember, CurrentMember) ??
            AdminRoutes.Events(Chapter);
        Response.Redirect(permittedRoute.Path);
    }
}