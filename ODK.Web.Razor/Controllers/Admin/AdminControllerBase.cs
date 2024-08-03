using Microsoft.AspNetCore.Authorization;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize(Roles = OdkRoles.Admin)]
public class AdminControllerBase : OdkControllerBase
{
    private readonly IRequestCache _requestCache;

    protected AdminControllerBase(IRequestCache requestCache)
    {
        _requestCache = requestCache; 
    }

    protected async Task<AdminServiceRequest> GetAdminServiceRequest(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        return new AdminServiceRequest(chapter.Id, MemberId);
    }
}
