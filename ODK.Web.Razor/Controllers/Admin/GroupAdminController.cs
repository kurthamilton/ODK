using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Chapters;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize]
public class GroupAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;

    public GroupAdminController(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    [HttpPost("admin/groups/{id:guid}/conversations/{conversationId:guid}/reply")]
    public async Task<IActionResult> ReplyToConversation(Guid id, Guid conversationId,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        var request = new AdminServiceRequest(id, MemberId);
        await _chapterAdminService.ReplyToConversation(request, conversationId, viewModel.Message ?? "");
        AddFeedback("Reply sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/description")]
    public async Task<IActionResult> UpdateDescription(Guid id, [FromForm] string description)
    {
        await _chapterAdminService.UpdateChapterDescription(
            new AdminServiceRequest(id, MemberId), description);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/instagram")]
    public async Task<IActionResult> UpdateInstagramName(Guid id, [FromForm] string? name)
    {
        await _chapterAdminService.UpdateChapterLinks(new AdminServiceRequest(id, MemberId), new UpdateChapterLinks
        {
            Instagram = name ?? ""
        });
        return RedirectToReferrer();
    }


    [HttpPost("admin/groups/{id:guid}/texts/register")]
    public async Task<IActionResult> UpdateRegisterText(Guid id, [FromForm] string text)
    {
        await _chapterAdminService.UpdateChapterDescription(
            new AdminServiceRequest(id, MemberId), text);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/texts/welcome")]
    public async Task<IActionResult> UpdateWelcomeText(Guid id, [FromForm] string text)
    {
        await _chapterAdminService.UpdateChapterDescription(
            new AdminServiceRequest(id, MemberId), text);
        return RedirectToReferrer();
    }
}
