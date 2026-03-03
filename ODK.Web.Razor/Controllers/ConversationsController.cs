using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Contact;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers;

[Authorize]
[ApiController]
public class ConversationsController : OdkControllerBase
{
    private readonly IContactService _contactService;

    public ConversationsController(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes,
        IContactService contactService)
        : base(requestStore, odkRoutes)
    {
        _contactService = contactService;
    }

    [HttpPost("conversations/{id:guid}/archive")]
    public async Task<IActionResult> ArchiveConversation(Guid id)
    {
        var result = await _contactService.ArchiveChapterConversation(MemberServiceRequest, id);
        AddFeedback(result, "Conversation archived");
        return RedirectToReferrer();
    }

    [HttpPost("conversations/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToConversation(
        Guid id,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        await _contactService.ReplyToChapterConversation(
            MemberServiceRequest,
            id,
            viewModel.Message ?? string.Empty);

        AddFeedback("Reply sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("conversations/{id:guid}/restore")]
    public async Task<IActionResult> Unarchive(Guid id)
    {
        var result = await _contactService.UnarchiveChapterConversation(MemberServiceRequest, id);
        AddFeedback(result, "Conversation restored");
        return RedirectToReferrer();
    }
}