using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Events;

namespace ODK.Web.Razor.Controllers;

public class EventsController : OdkControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }    

    [Authorize]
    [HttpPost("{chapterName}/Events/{id:guid}/Comments")]
    public async Task<IActionResult> AddComment(string chapterName, Guid id, EventCommentFormViewModel viewModel)
    {
        var result = await _eventService.AddComment(MemberId, id, viewModel.Text ?? "", viewModel.Parent);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
        }
        
        return RedirectToReferrer();
    }

    [Authorize]
    [HttpPost("{chapterName}/Events/{id:guid}/RSVP")]
    public async Task<IActionResult> UpdateResponse(string chapterName, Guid id, [FromForm] string? responseType)
    {
        if (Enum.TryParse<EventResponseType>(responseType, true, out var type))
        {
            await _eventService.UpdateMemberResponse(MemberId, id, type);
        }        

        return RedirectToReferrer();
    }
}
