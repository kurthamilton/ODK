using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Core.Utils;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Events;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

public class EventAdminController : AdminControllerBase
{
    private readonly IEventAdminService _eventAdminService;

    public EventAdminController(
        IEventAdminService eventAdminService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _eventAdminService = eventAdminService;
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/attendees/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberResponse(Guid chapterId, Guid id, Guid memberId,
        [FromForm] EventResponseType responseType)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _eventAdminService.UpdateMemberResponse(request, id, memberId, responseType);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/delete")]
    public async Task<IActionResult> DeleteEvent(Guid chapterId, Guid id)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _eventAdminService.DeleteEvent(request, id);
        AddFeedback("Event deleted", FeedbackType.Success);

        var chapter = await GetChapter();
        return Redirect(OdkRoutes.MemberGroups.Events(Platform, chapter));
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send")]
    public async Task<IActionResult> SendInvites(Guid chapterId, Guid id)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
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
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _eventAdminService.SendEventInviteeEmail(request, id,
            model.ResponseTypes, model.Subject ?? string.Empty, model.Body ?? string.Empty);
        AddFeedback("Update sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send/test")]
    public async Task<IActionResult> SendTestInvites(Guid chapterId, Guid id)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        var result = await _eventAdminService.SendEventInvites(request, id, true);
        AddFeedback(result, "Invites sent");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid chapterId, Guid id)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _eventAdminService.PublishEvent(request, id);
        AddFeedback("Event published", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/scheduled")]
    public async Task<IActionResult> UpdateScheduledEmail(
        Guid chapterId, Guid id, [FromForm] EventScheduledEmailFormViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
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
        var request = CreateMemberChapterServiceRequest(id);

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