using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Caching;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Controllers.Admin;

public class EventAdminController : AdminControllerBase
{
    private readonly IEventAdminService _eventAdminService;

    public EventAdminController(IEventAdminService eventAdminService, IRequestCache requestCache)
        : base(requestCache)
    {
        _eventAdminService = eventAdminService;
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Attendees/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberResponse(string chapterName, Guid id, Guid memberId, 
        [FromForm] EventResponseType responseType)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _eventAdminService.UpdateMemberResponse(request, id, memberId, responseType);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteEvent(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _eventAdminService.DeleteEvent(request, id);
        AddFeedback(new FeedbackViewModel("Event deleted", FeedbackType.Success));
        return Redirect($"/{chapterName}/Admin/Events");
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/Send")]
    public async Task<IActionResult> SendInvites(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        var result = await _eventAdminService.SendEventInvites(request, id);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/SendUpdate")]
    public async Task<IActionResult> SendUpdate(string chapterName, Guid id, EventUpdateViewModel model)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _eventAdminService.SendEventInviteeEmail(request, id, 
            model.ResponseTypes, model.Subject ?? "", model.Body ?? "");
        AddFeedback(new FeedbackViewModel("Update sent", FeedbackType.Success));

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/SendTest")]
    public async Task<IActionResult> SendTestInvites(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        var result = await _eventAdminService.SendEventInvites(request, id, true);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Invites sent", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Publish")]
    public async Task<IActionResult> PublishEvent(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _eventAdminService.PublishEvent(request, id);
        AddFeedback(new FeedbackViewModel("Event published", FeedbackType.Success));
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/ScheduledEmail")]
    public async Task<IActionResult> UpdateScheduledEmail(string chapterName, Guid id, EventScheduledEmailFormViewModel viewModel)
    {
        var request = await GetAdminServiceRequest(chapterName);
        var result = await _eventAdminService.UpdateScheduledEmail(
            request,
            id,
            viewModel.ScheduledEmailDate,
            viewModel.ScheduledEmailTime);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Scheduled email date updated", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }
}
