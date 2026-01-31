using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

public abstract class AdminPageModel : OdkPageModel
{
    private readonly Lazy<MemberChapterAdminServiceRequest> _memberChapterAdminServiceRequest;

    protected AdminPageModel()
    {
        _memberChapterAdminServiceRequest =
            new(() => MemberChapterAdminServiceRequest.Create(Securable, MemberChapterServiceRequest));
    }

    public MemberChapterAdminServiceRequest MemberChapterAdminServiceRequest => _memberChapterAdminServiceRequest.Value;

    public abstract ChapterAdminSecurable Securable { get; }    

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        if (!adminMember.HasAccessTo(Securable))
        {
            context.HttpContext.Response.Redirect(OdkRoutes.GroupAdmin.Events(Chapter));
            return;
        }

        await next();
    }
}