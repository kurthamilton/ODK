using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Core.Platforms;
using ODK.Services.Events;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Events;

namespace ODK.Web.Razor.Controllers;

public class EventsController : OdkControllerBase
{
    private readonly IEventService _eventService;
    private readonly IPlatformProvider _platformProvider;

    public EventsController(
        IEventService eventService,
        IPlatformProvider platformProvider)
    {
        _eventService = eventService;
        _platformProvider = platformProvider;
    }

    [Authorize]
    [HttpGet("events/{id:guid}/attend")]
    public async Task<IActionResult> AttendEvent(Guid id)
    {
        var result = await _eventService.UpdateMemberResponse(MemberId, id, EventResponseType.Yes);
        AddFeedback(result, "Attendance updated");

        var platform = _platformProvider.GetPlatform();
        var (chapter, @event) = await _eventService.GetEvent(id);
        return Redirect(OdkRoutes.Groups.Event(platform, chapter, id));
    }

    [Authorize]
    [HttpPost("events/{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, EventCommentFormViewModel viewModel)
    {
        var result = await _eventService.AddComment(MemberId, id, viewModel.Text ?? "", viewModel.Parent);
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
        var result = await _eventService.PayDeposit(MemberId, id, token ?? "");
        AddFeedback(result, "Deposit paid");
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/tickets/purchase")]
    public async Task<IActionResult> PurchaseTicket(Guid id, [FromForm] string? token)
    {
        var result = await _eventService.PurchaseTicket(MemberId, id, token ?? "");
        AddFeedback(result, "Ticket purchased");
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/tickets/complete")]
    public async Task<IActionResult> CompleteTicketPurchase(string chapterName, Guid id, [FromForm] string? token)
    {
        var result = await _eventService.PayTicketRemainder(MemberId, id, token ?? "");
        AddFeedback(result, "Ticket purchase complete");
        return RedirectToReferrer();
    }
}
