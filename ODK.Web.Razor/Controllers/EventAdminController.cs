﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Controllers;

[Authorize(Roles = "Admin")]
public class EventAdminController : OdkControllerBase
{
    private readonly IEventAdminService _eventAdminService;

    public EventAdminController(IEventAdminService eventAdminService)
    {
        _eventAdminService = eventAdminService;
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Attendees/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberResponse(Guid id, Guid memberId, [FromForm] EventResponseType responseType)
    {
        await _eventAdminService.UpdateMemberResponse(MemberId, id, memberId, responseType);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteEvent(string chapterName, Guid id)
    {
        await _eventAdminService.DeleteEvent(MemberId, id);
        AddFeedback(new FeedbackViewModel("Event deleted", FeedbackType.Success));
        return Redirect($"/{chapterName}/Admin/Events");
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/Send")]
    public IActionResult SendInvites(Guid id)
    {
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/SendUpdate")]
    public async Task<IActionResult> SendUpdate(Guid id, EventUpdateViewModel model)
    {
        await _eventAdminService.SendEventInviteeEmail(MemberId, id, model.ResponseTypes, model.Subject ?? "", model.Body ?? "");
        AddFeedback(new FeedbackViewModel("Update sent", FeedbackType.Success));

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/SendTest")]
    public async Task<IActionResult> SendTestInvites(Guid id)
    {
        ServiceResult result = await _eventAdminService.SendEventInvites(MemberId, id, true);
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
}
