using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
using ODK.Services.Caching;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventCreateModel : AdminPageModel
{
    private readonly IEventAdminService _eventAdminService;

    public EventCreateModel(IRequestCache requestCache, IEventAdminService eventAdminService)
        : base(requestCache)
    {
        _eventAdminService = eventAdminService;
    }

    public Guid? VenueId { get; private set; }

    public void OnGet(Guid? venueId = null)
    {
        VenueId = venueId;
    }

    public async Task<IActionResult> OnPostAsync([FromForm] EventFormSubmitViewModel viewModel)
    {
        var request = await GetAdminServiceRequest();
        var result = await _eventAdminService.CreateEvent(request, new CreateEvent
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
            TicketCost = viewModel.TicketCost,
            TicketDepositCost = viewModel.TicketDepositCost,
            Time = viewModel.Time,
            VenueId = viewModel.Venue
        }, viewModel.Draft);

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Event created", FeedbackType.Success));
        return Redirect($"/{Chapter.ShortName}/Admin/Events");
    }
}
