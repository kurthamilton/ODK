using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
using ODK.Services.Events;
using ODK.Services.Events.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Admin.Events;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IEventAdminService _eventAdminService;

    public CreateModel(IEventAdminService eventAdminService)
    {
        _eventAdminService = eventAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public Guid? VenueId { get; private set; }

    public void OnGet([FromQuery] Guid? venueId = null)
    {
        VenueId = venueId;
    }

    public async Task<IActionResult> OnPostAsync([FromForm] EventFormSubmitViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _eventAdminService.CreateEvent(request, new EventCreateModel
        {
            AttendeeLimit = viewModel.AttendeeLimit,
            Date = viewModel.Date,
            Description = viewModel.Description,
            EndTime = TimeSpanUtils.FromString(viewModel.EndTime),
            Hosts = viewModel.Hosts,
            ImageUrl = viewModel.ImageUrl,
            IsPublic = false,
            Name = viewModel.Name,
            RsvpDeadline = viewModel.RsvpDeadline,
            RsvpDisabled = viewModel.RsvpDisabled,
            TicketCost = viewModel.TicketCost,
            TicketDepositCost = viewModel.TicketDepositCost,
            Time = viewModel.Time,
            VenueId = viewModel.Venue
        }, viewModel.Draft);

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Event created", FeedbackType.Success);
        var path = OdkRoutes.GroupAdmin.Events(Chapter).Path;
        return Redirect(path);
    }
}