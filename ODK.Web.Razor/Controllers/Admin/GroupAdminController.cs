using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Topics;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize]
public class GroupAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;

    public GroupAdminController(
        IChapterAdminService chapterAdminService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _chapterAdminService = chapterAdminService;
    }

    [HttpPost("admin/groups/{id:guid}/conversations")]
    public async Task<IActionResult> StartConversation(Guid id,
        [FromForm] ChapterAdminStartConversationFormViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(id);
        await _chapterAdminService.StartConversation(request, viewModel.MemberId,
            viewModel.Subject ?? string.Empty, viewModel.Message ?? string.Empty);
        AddFeedback("Message sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/conversations/{conversationId:guid}/reply")]
    public async Task<IActionResult> ReplyToConversation(Guid id, Guid conversationId,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(id);
        var result = await _chapterAdminService.ReplyToConversation(request, conversationId, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/description")]
    public async Task<IActionResult> UpdateDescription(Guid id, [FromForm] string description)
    {
        var request = CreateMemberChapterServiceRequest(id);
        await _chapterAdminService.UpdateChapterDescription(request, description);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/texts/register")]
    public async Task<IActionResult> UpdateRegisterText(Guid id, [FromForm] string text)
    {
        var request = CreateMemberChapterServiceRequest(id);
        await _chapterAdminService.UpdateChapterDescription(request, text);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{id:guid}/topics")]
    public async Task<IActionResult> UpdateTopics(Guid id, [FromForm] TopicPickerViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(id);
        var result = await _chapterAdminService.UpdateChapterTopics(request, viewModel.TopicIds ?? []);
        AddFeedback(result, "Topics updated");
        return RedirectToReferrer();
    }
}
