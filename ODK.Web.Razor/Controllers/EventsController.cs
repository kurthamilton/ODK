using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Chapters;
using ODK.Services.Events;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Events;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

public class EventsController : OdkControllerBase
{
    private readonly IChapterService _chapterService;
    private readonly IEventService _eventService;

    public EventsController(
        IEventService eventService,
        IRequestStore requestStore,
        IChapterService chapterService)
        : base(requestStore)
    {
        _chapterService = chapterService;
        _eventService = eventService;
    }

    // Backwards compatibility for old emails
    [Authorize]
    [HttpGet("events/{id:guid}/attend")]
    public async Task<IActionResult> EmailRsvpLegacy(Guid id)
    {
        var chapter = await _chapterService.GetByEventId(id);
        var url = OdkRoutes.Groups.EventAttend(Platform, chapter, id);
        return RedirectPermanent(url);
    }

    [Authorize]
    [HttpGet("{chapterName}/events/{id:guid}/rsvp")]
    [HttpGet("groups/{slug}/events/{id:guid}/rsvp")]
    public Task<IActionResult> EmailRsvp(Guid id) => AttendEvent(id);

    [Authorize]
    [HttpPost("groups/{chapterId:guid}/events/{id:guid}/comments")]
    public async Task<IActionResult> AddComment(
        Guid chapterId, Guid id, [FromForm] EventCommentFormViewModel viewModel)
    {
        var chapter = await GetChapter();
        var result = await _eventService.AddComment(
            MemberServiceRequest, id, chapter, viewModel.Text ?? string.Empty, viewModel.Parent);
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
        var result = await _eventService.UpdateMemberResponse(MemberServiceRequest, id, responseType);
        if (!result.Success)
        {
            AddFeedback(result);
        }

        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/waiting-list")]
    public async Task<IActionResult> JoinWaitinglist(Guid id)
    {
        var result = await _eventService.JoinWaitingList(id, MemberId);
        AddFeedback(result, "You have joined the waiting list");
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("events/{id:guid}/waiting-list/leave")]
    public async Task<IActionResult> LeaveWaitinglist(Guid id)
    {
        var result = await _eventService.LeaveWaitingList(id, MemberId);
        AddFeedback(result, "You have left the waiting list");
        return RedirectToReferrer();
    }

    private async Task<IActionResult> AttendEvent(Guid id)
    {
        var result = await _eventService.UpdateMemberResponse(MemberServiceRequest, id, EventResponseType.Yes);
        AddFeedback(result, "Attendance updated");

        var chapter = await GetChapter();
        return Redirect(OdkRoutes.Groups.Event(Platform, chapter, id));
    }
}