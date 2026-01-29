using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Contact;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Contact;

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
        IChapterService chapterService,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterService = chapterService;
        _contactService = contactService;
        _memberService = memberService;
    }

    [HttpPost("groups/{chapterId:guid}/contact")]
    public async Task<IActionResult> Contact(Guid chapterId, [FromForm] ContactFormViewModel viewModel)
    {
        await _contactService.SendChapterContactMessage(
            ServiceRequest,
            chapterId,
            viewModel.EmailAddress ?? string.Empty,
            viewModel.Message ?? string.Empty,
            viewModel.Recaptcha ?? string.Empty);

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
            viewModel.Subject ?? string.Empty,
            viewModel.Message ?? string.Empty,
            viewModel.Recaptcha ?? string.Empty);

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
            viewModel.Message ?? string.Empty);

        AddFeedback("Reply sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [SkipRequestStoreMiddleware]
    [HttpGet("groups/{chapterId:guid}/image")]
    public Task<IActionResult> Image(Guid chapterId)
        => HandleVersionedRequest(version => _chapterService.GetChapterImage(version, chapterId), ChapterImageResult);

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid chapterId, [FromForm] string reason)
    {
        var request = MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);
        var result = await _memberService.LeaveChapter(request, reason);
        AddFeedback(result, "You have left the group");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        return Redirect(OdkRoutes.GroupAdmin.Index());
    }

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/profile")]
    public async Task<IActionResult> UpdateChapterProfile(Guid chapterId,
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel)
    {
        var model = new UpdateMemberChapterProfile
        {
            Properties = profileViewModel.Properties.Select(x => new UpdateMemberProperty
            {
                ChapterPropertyId = x.ChapterPropertyId,
                Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
                    ? x.OtherValue ?? string.Empty
                    : x.Value ?? string.Empty
            })
        };

        var request = MemberChapterServiceRequest;

        var result = await _memberService.UpdateMemberChapterProfile(request, model);
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