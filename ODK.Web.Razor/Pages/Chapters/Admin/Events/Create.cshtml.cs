using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
using ODK.Services.Events;
using ODK.Services.Events.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Admin.Events;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class CreateModel : AdminPageModel
{
    private readonly IEventAdminService _eventAdminService;

    public CreateModel(IEventAdminService eventAdminService)
    {
        _eventAdminService = eventAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public Guid? ChapterVenueId { get; private set; }

    public void OnGet(Guid? chapterVenueId = null)
    {
        ChapterVenueId = chapterVenueId;
    }

    public async Task<IActionResult> OnPostAsync([FromForm] EventFormSubmitViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _eventAdminService.CreateEvent(request, new EventCreateModel
        {
            AttendeeLimit = viewModel.AttendeeLimit,
            ChapterVenueId = viewModel.ChapterVenue,
            Date = viewModel.Date,
            DescriptionHtml = viewModel.DescriptionHtml,
            EndTime = TimeSpanUtils.FromString(viewModel.EndTime),
            Hosts = viewModel.Hosts ?? [],
            ImageUrl = viewModel.ImageUrl,
            IsPublic = viewModel.Public,
            Name = viewModel.Name,
            RsvpDeadline = viewModel.RsvpDeadline,
            RsvpDisabled = viewModel.RsvpDisabled,
            TicketCost = viewModel.TicketCost,
            TicketDepositCost = viewModel.TicketDepositCost,
            Time = viewModel.Time
        }, viewModel.Draft);

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Event created", FeedbackType.Success);
        return Redirect(AdminRoutes.Events(Chapter).Path);
    }
}