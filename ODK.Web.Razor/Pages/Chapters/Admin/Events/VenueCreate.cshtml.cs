using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Venues;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenueCreateModel : AdminPageModel
{
    private readonly IVenueAdminService _venueAdminService;

    public VenueCreateModel(IRequestCache requestCache, IVenueAdminService venueAdminService) 
        : base(requestCache)
    {
        _venueAdminService = venueAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(VenueFormViewModel viewModel)
    {
        ServiceResult result = await _venueAdminService.CreateVenue(CurrentMemberId, new CreateVenue
        {
            Address = viewModel.Address,
            ChapterId = Chapter.Id,
            MapQuery = viewModel.MapQuery,
            Name = viewModel.Name
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Venue created", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Events/Venues");
    }
}
