using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Venues;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenueModel : VenueAdminPageModel
{
    public VenueModel(IRequestCache requestCache, IVenueAdminService venueAdminService) 
        : base(requestCache, venueAdminService)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(VenueFormViewModel viewModel)
    {
        ServiceResult result = await VenueAdminService.UpdateVenue(CurrentMemberId, Venue.Id, new CreateVenue
        {
            Address = viewModel.Address,
            ChapterId = Chapter.Id,
            MapQuery = viewModel.MapQuery,
            Name = viewModel.Name ?? ""
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Venue updated", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Events/Venues");
    }
}
