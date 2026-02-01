using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Services.Chapters;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Controllers.Admin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class ChapterSiteAdminController : AdminControllerBase
{
    private readonly IChapterSiteAdminService _chapterSiteAdminService;
    public ChapterSiteAdminController(
        IChapterSiteAdminService chapterSiteAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterSiteAdminService = chapterSiteAdminService;
    }

    [HttpPost("/siteadmin/groups/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var request = MemberServiceRequest;
        var result = await _chapterSiteAdminService.ApproveChapter(request, id);
        AddFeedback(result, "Group approved");
        return RedirectToReferrer();
    }

    [HttpPost("/siteadmin/groups/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = MemberServiceRequest;
        var result = await _chapterSiteAdminService.DeleteChapter(request, id);
        AddFeedback(result, "Chapter deleted");
        return RedirectToReferrer();
    }
}