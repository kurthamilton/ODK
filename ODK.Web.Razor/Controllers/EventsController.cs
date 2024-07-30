using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpPost("{chapter}/Events/{Id}/Comments")]
    public async Task<IActionResult> AddComment(Guid id, string chapter, EventCommentFormViewModel viewModel)
    {
        var result = await _eventService.AddComment(MemberId, id, viewModel.Text ?? "", viewModel.Parent);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
        }
        
        return RedirectToReferrer();
    }
}
