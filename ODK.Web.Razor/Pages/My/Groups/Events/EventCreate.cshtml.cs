using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class EventCreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterService _chapterService;
    private readonly IEventAdminService _eventAdminService;

    public EventCreateModel(
        IEventAdminService eventAdminService,
        IChapterService chapterService)
    {
        _chapterService = chapterService;
        _eventAdminService = eventAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync([FromForm] EventFormSubmitViewModel viewModel)
    {
        var request = new AdminServiceRequest(ChapterId, CurrentMemberId);
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

        var chapter = await _chapterService.GetChapterById(ChapterId);
        AddFeedback(new FeedbackViewModel("Event created", FeedbackType.Success));
        var url = OdkRoutes2.Groups.Events(chapter);
        return Redirect(url);
    }
}
