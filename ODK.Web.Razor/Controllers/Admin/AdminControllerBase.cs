using Microsoft.AspNetCore.Authorization;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize(Roles = OdkRoles.Admin)]
public class AdminControllerBase : OdkControllerBase
{
    private readonly IRequestCache _requestCache;
    private readonly IRequestStore _requestStore;

    protected AdminControllerBase(
        IRequestCache requestCache,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _requestCache = requestCache; 
        _requestStore = requestStore;
    }

    protected async Task<MemberChapterServiceRequest> GetAdminServiceRequest(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(_requestStore.Platform, chapterName);
        return new MemberChapterServiceRequest(chapter.Id, MemberServiceRequest);
    }
}
