using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Controllers.Admin;

public class EventAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IEventAdminService _eventAdminService;
    private readonly IPlatformProvider _platformProvider;

    public EventAdminController(
        IEventAdminService eventAdminService, 
        IRequestCache requestCache,
        IPlatformProvider platformProvider,
        IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _eventAdminService = eventAdminService;
        _platformProvider = platformProvider;
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/attendees/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberResponse(Guid chapterId, Guid id, Guid memberId, 
        [FromForm] EventResponseType responseType)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _eventAdminService.UpdateMemberResponse(request, id, memberId, responseType);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/delete")]
    public async Task<IActionResult> DeleteEvent(Guid chapterId, Guid id)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _eventAdminService.DeleteEvent(request, id);
        AddFeedback("Event deleted", FeedbackType.Success);

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterAdminService.GetChapter(request);
        return Redirect(OdkRoutes.MemberGroups.Events(platform, chapter));
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send")]
    public async Task<IActionResult> SendInvites(Guid chapterId, Guid id)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        var result = await _eventAdminService.SendEventInvites(request, id);
        if (!result.Success)
        {
            AddFeedback(result);
        }

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send/update")]
    public async Task<IActionResult> SendUpdate(Guid chapterId, Guid id, EventUpdateViewModel model)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _eventAdminService.SendEventInviteeEmail(request, id, 
            model.ResponseTypes, model.Subject ?? "", model.Body ?? "");
        AddFeedback("Update sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send/test")]
    public async Task<IActionResult> SendTestInvites(Guid chapterId, Guid id)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        var result = await _eventAdminService.SendEventInvites(request, id, true);
        AddFeedback(result, "Invites sent");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid chapterId, Guid id)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _eventAdminService.PublishEvent(request, id);
        AddFeedback("Event published", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/scheduled")]
    public async Task<IActionResult> UpdateScheduledEmail(Guid chapterId, Guid id, EventScheduledEmailFormViewModel viewModel)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        var result = await _eventAdminService.UpdateScheduledEmail(
            request,
            id,
            viewModel.ScheduledEmailDate);
        AddFeedback(result, "Scheduled email date updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{id:guid}/events/settings")]
    public async Task<IActionResult> UpdateEventSettings(Guid id, 
        [FromForm] EventSettingsFormSubmitViewModel viewModel)
    {
        var request = new AdminServiceRequest(id, MemberId);
        
        await _eventAdminService.UpdateEventSettings(request, new UpdateEventSettings
        {
            DefaultDayOfWeek = viewModel.DefaultDayOfWeek,
            DefaultDescription = viewModel.DefaultDescription,
            DefaultEndTime = TimeSpanUtils.FromString(viewModel.DefaultEndTime),
            DefaultScheduledEmailDayOfWeek = viewModel.DefaultScheduledEmailDayOfWeek,
            DefaultScheduledEmailTimeOfDay = TimeSpanUtils.FromString(viewModel.DefaultScheduledEmailTimeOfDay),
            DefaultStartTime = TimeSpanUtils.FromString(viewModel.DefaultStartTime),
            DisableComments = viewModel.DisableComments
        });

        AddFeedback(new FeedbackViewModel("Event settings updated", FeedbackType.Success));

        return RedirectToReferrer();
    }
}
