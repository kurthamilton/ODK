using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Events;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

public class EventsController : OdkControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(
        IEventService eventService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _eventService = eventService;
    }

    [Authorize]
    [HttpGet("events/{id:guid}/attend")]
    public async Task<IActionResult> AttendEvent(Guid id)
    {
        var result = await _eventService.UpdateMemberResponse(MemberId, id, EventResponseType.Yes);
        AddFeedback(result, "Attendance updated");

        var (chapter, @event) = await _eventService.GetEvent(id);
        return Redirect(OdkRoutes.Groups.Event(Platform, chapter, id));
    }

    [Authorize]
    [HttpPost("events/{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, EventCommentFormViewModel viewModel)
    {
        var result = await _eventService.AddComment(MemberServiceRequest, id, viewModel.Text ?? string.Empty, viewModel.Parent);
        if (!result.Success)
        {
            AddFeedback(result);
        }

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/rsvp")]
    public async Task<IActionResult> UpdateResponse(Guid id, [FromForm] EventResponseType responseType)
    {
        var result = await _eventService.UpdateMemberResponse(MemberId, id, responseType);
        if (!result.Success)
        {
            AddFeedback(result);
        }

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/tickets/deposit")]
    public async Task<IActionResult> PayDeposit(Guid id, [FromForm] string? token)
    {
        var result = await _eventService.PayDeposit(MemberId, id, token ?? string.Empty);
        AddFeedback(result, "Deposit paid");
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/tickets/purchase")]
    public async Task<IActionResult> PurchaseTicket(Guid id, [FromForm] string? token)
    {
        var result = await _eventService.PurchaseTicket(MemberId, id, token ?? string.Empty);
        AddFeedback(result, "Ticket purchased");
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/tickets/complete")]
    public async Task<IActionResult> CompleteTicketPurchase(string chapterName, Guid id, [FromForm] string? token)
    {
        var result = await _eventService.PayTicketRemainder(MemberId, id, token ?? string.Empty);
        AddFeedback(result, "Ticket purchase complete");
        return RedirectToReferrer();
    }
}
