using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Core.Utils;
using ODK.Services;
using ODK.Services.Events;
using ODK.Services.Security;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Events;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers.Admin;

public class EventAdminController : AdminControllerBase
{
    private readonly IEventAdminService _eventAdminService;

    public EventAdminController(
        IEventAdminService eventAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _eventAdminService = eventAdminService;
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/attendees/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberResponse(Guid chapterId, Guid id, Guid memberId,
        [FromForm] EventResponseType responseType)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberEventResponses,
            MemberChapterServiceRequest);
        var result = await _eventAdminService.UpdateMemberResponse(request, id, memberId, responseType);
        AddFeedback(result, "Response updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{eventId:guid}/comments/{id:guid}/hide")]
    public async Task<IActionResult> HideEventComment(Guid chapterId, Guid eventId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        await _eventAdminService.SetEventCommentVisibility(request, eventId, id, hidden: true);
        AddFeedback("Comment hidden", FeedbackType.Success);

        return Redirect(OdkRoutes.GroupAdmin.EventComments(Chapter, eventId).Path);
    }

    [HttpPost("groups/{chapterId:guid}/events/{eventId:guid}/comments/{id:guid}/show")]
    public async Task<IActionResult> ShowEventComment(Guid chapterId, Guid eventId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        await _eventAdminService.SetEventCommentVisibility(request, eventId, id, hidden: false);
        AddFeedback("Comment unhidden", FeedbackType.Success);

        return Redirect(OdkRoutes.GroupAdmin.EventComments(Chapter, eventId).Path);
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/delete")]
    public async Task<IActionResult> DeleteEvent(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        await _eventAdminService.DeleteEvent(request, id);
        AddFeedback("Event deleted", FeedbackType.Success);

        return Redirect(OdkRoutes.GroupAdmin.Events(Chapter).Path);
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send")]
    public async Task<IActionResult> SendInvites(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
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
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        await _eventAdminService.SendEventInviteeEmail(request, id,
            model.ResponseTypes, model.Subject ?? string.Empty, model.Body ?? string.Empty);
        AddFeedback("Update sent", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/send/test")]
    public async Task<IActionResult> SendTestInvites(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        var result = await _eventAdminService.SendEventInvites(request, id, true);
        AddFeedback(result, "Invites sent");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        await _eventAdminService.PublishEvent(request, id);
        AddFeedback("Event published", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/invites/scheduled")]
    public async Task<IActionResult> UpdateScheduledEmail(
        Guid chapterId, Guid id, [FromForm] EventScheduledEmailFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Events,
            MemberChapterServiceRequest);
        var result = await _eventAdminService.UpdateScheduledEmail(
            request,
            id,
            viewModel.ScheduledEmailDate);
        AddFeedback(result, "Scheduled email date updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/events/settings")]
    public async Task<IActionResult> UpdateEventSettings(Guid chapterId,
        [FromForm] EventSettingsFormSubmitViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.EventSettings,
            MemberChapterServiceRequest);

        await _eventAdminService.UpdateEventSettings(request, new EventSettingsUpdateModel
        {
            DefaultDayOfWeek = viewModel.DefaultDayOfWeek,
            DefaultDescription = viewModel.DefaultDescription,
            DefaultEndTime = TimeSpanUtils.FromString(viewModel.DefaultEndTime),
            DefaultScheduledEmailDayOfWeek = viewModel.DefaultScheduledEmailDayOfWeek,
            DefaultScheduledEmailTimeOfDay = TimeSpanUtils.FromString(viewModel.DefaultScheduledEmailTimeOfDay),
            DefaultStartTime = TimeSpanUtils.FromString(viewModel.DefaultStartTime),
            DisableComments = viewModel.DisableComments
        });

        AddFeedback("Event settings updated", FeedbackType.Success);

        return RedirectToReferrer();
    }
}