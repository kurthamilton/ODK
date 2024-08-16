using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventModel : EventAdminPageModel
{
    public EventModel(IRequestCache requestCache, IEventAdminService eventAdminService) 
        : base(requestCache, eventAdminService)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(Guid id, [FromForm] EventFormViewModel viewModel)
    {
        var request = await GetAdminServiceRequest();
        var result = await EventAdminService.UpdateEvent(request, id, new CreateEvent
        {
            AttendeeLimit = viewModel.AttendeeLimit,
            Date = viewModel.Date,
            Description = viewModel.Description,
            Hosts = viewModel.Hosts,
            ImageUrl = viewModel.ImageUrl,
            IsPublic = viewModel.Public,
            Name = viewModel.Name,
            TicketCost = viewModel.TicketCost,
            TicketDepositCost = viewModel.TicketDepositCost,
            Time = viewModel.Time,
            VenueId = viewModel.Venue
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Event updated", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Events");
    }
}
