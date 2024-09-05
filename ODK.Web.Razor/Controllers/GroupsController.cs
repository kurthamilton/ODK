using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Contact;
using ODK.Services.Members;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Contact;

namespace ODK.Web.Razor.Controllers;

public class GroupsController : OdkControllerBase
{
    private readonly IContactService _contactService;
    private readonly IMemberService _memberService;
    private readonly IPlatformProvider _platformProvider;

    public GroupsController(
        IMemberService memberService,
        IPlatformProvider platformProvider,
        IContactService contactService)
    {
        _contactService = contactService;
        _memberService = memberService;
        _platformProvider = platformProvider;
    }

    [HttpPost("groups/{id:guid}/contact")]
    public async Task<IActionResult> Contact(Guid id, [FromForm] ContactFormViewModel viewModel)
    {
        await _contactService.SendChapterContactMessage(id, 
            viewModel.EmailAddress ?? "", 
            viewModel.Message ?? "", 
            viewModel.Recaptcha ?? "");

        AddFeedback("Your message has been sent. Thank you for getting in touch.", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/conversations")]
    public async Task<IActionResult> Conversation(Guid chapterId, [FromForm] ConversationFormViewModel viewModel)
    {
        await _contactService.StartChapterConversation(
            MemberId,
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
            MemberId,
            conversationId,
            viewModel.Message ?? "");

        AddFeedback("Reply sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("groups/{id:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id, [FromForm] string reason)
    {
        var result = await _memberService.LeaveChapter(MemberId, id, reason);
        AddFeedback(result, "You have left the group");
                
        if (!result.Success)
        {
            return RedirectToReferrer();            
        }                

        var platform = _platformProvider.GetPlatform();
        return Redirect(OdkRoutes2.MemberGroups.Index(platform));
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
}
