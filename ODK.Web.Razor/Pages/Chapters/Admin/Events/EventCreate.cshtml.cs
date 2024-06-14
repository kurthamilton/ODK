using Microsoft.AspNetCore.Mvc;
using ODK.Services;
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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync([FromForm] EventFormViewModel viewModel)
    {
        ServiceResult result = await _eventAdminService.CreateEvent(CurrentMemberId, new CreateEvent
        {
            ChapterId = Chapter.Id,
            Date = viewModel.Date,
            Description = viewModel.Description,
            ImageUrl = viewModel.ImageUrl,
            IsPublic = viewModel.Public,
            Name = viewModel.Name,
            Time = viewModel.Time,
            VenueId = viewModel.Venue
        });
        
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Event created", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Events");
    }
}
