using Microsoft.AspNetCore.Authorization;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize(Roles = OdkRoles.Admin)]
public class AdminControllerBase : OdkControllerBase
{
    protected AdminControllerBase(IRequestStore requestStore)
        : base(requestStore)
    {
    }

    protected MemberChapterServiceRequest AdminServiceRequest(Guid chapterId)
        => new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
}