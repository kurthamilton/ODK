using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
using ODK.Services.Events;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Event;

public class IndexModel : OdkGroupAdminPageModel
{
    private readonly IEventAdminService _eventAdminService;

    public IndexModel(IEventAdminService eventAdminService)
    {
        _eventAdminService = eventAdminService;
    }

    public Guid EventId { get; private set; }

    public void OnGet(Guid eventId)
    {
        EventId = eventId;
    }

    public async Task<IActionResult> OnPostAsync(Guid eventId, [FromForm] EventFormSubmitViewModel viewModel)
    {
        var result = await _eventAdminService.UpdateEvent(AdminServiceRequest, eventId, new CreateEvent
        {
            AttendeeLimit = viewModel.AttendeeLimit,
            Date = viewModel.Date,
            Description = viewModel.Description,
            EndTime = TimeSpanUtils.FromString(viewModel.EndTime),
            Hosts = viewModel.Hosts,
            ImageUrl = viewModel.ImageUrl,
            IsPublic = viewModel.Public,
            Name = viewModel.Name,
            RsvpDeadline = viewModel.RsvpDeadline,
            RsvpDisabled = viewModel.RsvpDisabled,
            TicketCost = viewModel.TicketCost,
            TicketDepositCost = viewModel.TicketDepositCost,
            Time = viewModel.Time,
            VenueId = viewModel.Venue
        });

        AddFeedback(result, "Event updated");

        return result.Success
            ? RedirectToPage()
            : Page();
    }
}
