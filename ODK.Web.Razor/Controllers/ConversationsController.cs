using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Contact;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

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

    [HttpPost("conversations/{id:guid}/unarchive")]
    public async Task<IActionResult> Unarchive(Guid id)
    {
        var result = await _contactService.UnarchiveChapterConversation(MemberServiceRequest, id);
        AddFeedback(result, "Conversation unarchived");
        return RedirectToReferrer();
    }
}