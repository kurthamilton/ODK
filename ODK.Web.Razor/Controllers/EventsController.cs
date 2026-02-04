using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Chapters;
using ODK.Services.Events;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Events;

namespace ODK.Web.Razor.Controllers;

public class EventsController : OdkControllerBase
{
    private readonly IChapterService _chapterService;
    private readonly IEventService _eventService;

    public EventsController(
        IEventService eventService,
        IRequestStore requestStore,
        IChapterService chapterService,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterService = chapterService;
        _eventService = eventService;
    }

    // Backwards compatibility for URLs using the EventId
    [HttpGet("{chapterName}/events/{id:guid}")]
    public async Task<IActionResult> EventLegacy(string chapterName, Guid id)
    {
        var @event = await _eventService.GetById(id);
        var url = OdkRoutes.Groups.EventAttend(Chapter, @event.Shortcode);
        return RedirectPermanent(url);
    }

    [Obsolete]
    [Authorize]
    [HttpGet("events/{id:guid}/attend")]
    public async Task<IActionResult> EmailRsvpLegacyLegacy(Guid id)
    {
        var chapter = await _chapterService.GetByEventId(ServiceRequest, id);
        var @event = await _eventService.GetById(id);
        var url = OdkRoutes.Groups.EventAttend(chapter, @event.Shortcode);
        return RedirectPermanent(url);
    }

    [Authorize]
    [HttpGet("{chapterName}/events/{id:guid}/rsvp")]
    [HttpGet("groups/{slug}/events/{id:guid}/rsvp")]
    public async Task<IActionResult> EmailRsvpLegacy(Guid id)
    {
        var @event = await _eventService.GetById(id);
        var url = OdkRoutes.Groups.EventAttend(Chapter, @event.Shortcode);
        return RedirectPermanent(url);
    }

    [Authorize]
    [HttpGet("{chapterName}/events/{shortcode}/rsvp")]
    [HttpGet("groups/{slug}/events/{shortcode}/rsvp")]
    public async Task<IActionResult> EmailRsvp(string shortcode)
    {
        var result = await _eventService.UpdateMemberResponse(
            MemberServiceRequest, shortcode, EventResponseType.Yes, adminMemberId: null);
        AddFeedback(result, "Attendance updated");

        return Redirect(OdkRoutes.Groups.Event(Chapter, shortcode));
    }

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/comments")]
    public async Task<IActionResult> AddComment(
        Guid chapterId, Guid id, [FromForm] EventCommentFormViewModel viewModel)
    {
        var result = await _eventService.AddComment(
            MemberChapterServiceRequest, id, viewModel.Text ?? string.Empty, viewModel.Parent);
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
        var result = await _eventService.UpdateMemberResponse(
            MemberServiceRequest, id, responseType, adminMemberId: null);
        if (!result.Success)
        {
            AddFeedback(result);
        }

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/wait-list")]
    public async Task<IActionResult> JoinWaitlist(Guid id)
    {
        var result = await _eventService.JoinWaitlist(id, MemberId);
        AddFeedback(result, "You have joined the waiting list");
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/wait-list/leave")]
    public async Task<IActionResult> LeaveWaitlist(Guid id)
    {
        var result = await _eventService.LeaveWaitlist(id, MemberId);
        AddFeedback(result, "You have left the waiting list");
        return RedirectToReferrer();
    }
}