using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
using ODK.Services.Events;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Events;

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
        var result = await _eventAdminService.CreateEvent(request, new CreateEvent
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
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Event created", FeedbackType.Success));
        var url = OdkRoutes.GroupAdmin.Events(Chapter);
        return Redirect(url);
    }
}