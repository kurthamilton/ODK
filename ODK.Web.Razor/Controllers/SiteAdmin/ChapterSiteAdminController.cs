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
    private readonly IChapterAdminService _chapterAdminService;

    public ChapterSiteAdminController(
        IChapterAdminService chapterAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterAdminService = chapterAdminService;
    }

    [HttpPost("/siteadmin/groups/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var request = CreateMemberChapterServiceRequest(id);
        var result = await _chapterAdminService.ApproveChapter(request);
        AddFeedback(result, "Group approved");
        return RedirectToReferrer();
    }

    [HttpPost("/siteadmin/groups/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = CreateMemberChapterServiceRequest(id);
        var result = await _chapterAdminService.DeleteChapter(request);
        AddFeedback(result, "Chapter deleted");
        return RedirectToReferrer();
    }
}