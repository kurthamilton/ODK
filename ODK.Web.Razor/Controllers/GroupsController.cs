using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Contact;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Contact;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

public class GroupsController : OdkControllerBase
{
    private readonly IChapterService _chapterService;
    private readonly IContactService _contactService;
    private readonly IMemberService _memberService;

    public GroupsController(
        IMemberService memberService,
        IRequestStore requestStore,
        IContactService contactService,
        IChapterService chapterService)
        : base(requestStore)
    {
        _chapterService = chapterService;
        _contactService = contactService;
        _memberService = memberService;
    }

    [HttpPost("groups/{id:guid}/contact")]
    public async Task<IActionResult> Contact(Guid id, [FromForm] ContactFormViewModel viewModel)
    {
        await _contactService.SendChapterContactMessage(
            ServiceRequest,
            id, 
            viewModel.EmailAddress ?? "", 
            viewModel.Message ?? "", 
            viewModel.Recaptcha ?? "");

        AddFeedback("Your message has been sent. Thank you for getting in touch.", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpGet("groups/available")]
    public async Task<bool> Available(string name)
    {
        return await _chapterService.NameIsAvailable(name);
    }

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/conversations")]
    public async Task<IActionResult> StartConversation(Guid chapterId, [FromForm] ConversationFormViewModel viewModel)
    {
        await _contactService.StartChapterConversation(
            MemberServiceRequest,
            chapterId,
            viewModel.Subject ?? "",
            viewModel.Message ?? "",
            viewModel.Recaptcha ?? "");

        AddFeedback("Your message has been sent. Thank you for getting in touch.", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/conversations/{conversationId:guid}/reply")]
    public async Task<IActionResult> ReplyToConversation(Guid chapterId, Guid conversationId, 
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        await _contactService.ReplyToChapterConversation(
            MemberServiceRequest,
            conversationId,
            viewModel.Message ?? "");

        AddFeedback("Reply sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpGet("groups/{id:guid}/image")]
    public Task<IActionResult> Image(Guid id)
        => HandleVersionedRequest(version => _chapterService.GetChapterImage(version, id), ChapterImageResult);

    [Authorize]
    [HttpPost("groups/{id:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id, [FromForm] string reason)
    {
        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        var result = await _memberService.LeaveChapter(request, reason);
        AddFeedback(result, "You have left the group");
                
        if (!result.Success)
        {
            return RedirectToReferrer();            
        }                

        return Redirect(OdkRoutes.MemberGroups.Index(Platform));
    }

    [Authorize]
    [HttpPost("groups/{id:guid}/profile")]
    public async Task<IActionResult> UpdateChapterProfile(Guid id,
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel)
    {        
        var model = new UpdateMemberChapterProfile
        {
            Properties = profileViewModel.Properties.Select(x => new UpdateMemberProperty
            {
                ChapterPropertyId = x.ChapterPropertyId,
                Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
                    ? x.OtherValue ?? ""
                    : x.Value ?? ""
            })
        };

        var memberId = User.MemberId();

        var result = await _memberService.UpdateMemberChapterProfile(memberId, id, model);
        AddFeedback(result, "Profile updated");
        return result.Success ? RedirectToReferrer() : View();
    }

    protected IActionResult ChapterImageResult(ChapterImage? image)
    {
        if (image == null)
        {
            return NoContent();
        }

        return File(image.ImageData, image.MimeType);
    }
}
