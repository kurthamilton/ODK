using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Contact;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Contact;
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

    [HttpPost("site-conversations/{id:guid}/archive")]
    public async Task<IActionResult> ArchiveSiteConversation(Guid id)
    {
        var result = await _contactService.ArchiveSiteConversation(MemberServiceRequest, id);
        AddFeedback(result, "Conversation archived");
        return RedirectToReferrer();
    }

    [HttpPost("site-conversations/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToSiteConversation(
        Guid id,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        var result = await _contactService.ReplyToSiteConversation(
            MemberServiceRequest,
            id,
            viewModel.Message ?? string.Empty);

        AddFeedback(result, "Reply sent");

        return RedirectToReferrer();
    }

    [HttpPost("site-conversations/{id:guid}/restore")]
    public async Task<IActionResult> UnarchiveSiteConversation(Guid id)
    {
        var result = await _contactService.UnarchiveSiteConversation(MemberServiceRequest, id);
        AddFeedback(result, "Conversation restored");
        return RedirectToReferrer();
    }

    [HttpPost("site-conversations/start")]
    public async Task<IActionResult> StartSiteConversation([FromForm] SiteConversationFormViewModel viewModel)
    {
        var result = await _contactService.StartSiteConversation(
            MemberServiceRequest,
            viewModel.Subject ?? string.Empty,
            viewModel.Message ?? string.Empty);

        AddFeedback(result, "Message sent");

        return result.Success
            ? Redirect(OdkRoutes.Account.SiteConversations())
            : RedirectToReferrer();
    }
}
