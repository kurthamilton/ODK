using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize]
public class GroupAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;

    public GroupAdminController(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    [HttpPost("admin/groups/{id:guid}/description")]
    public async Task<IActionResult> UpdateDescription(Guid id, [FromForm] string description)
    {
        await _chapterAdminService.UpdateChapterDescription(
            new AdminServiceRequest(id, MemberId), description);
        return RedirectToReferrer();
    }
}
