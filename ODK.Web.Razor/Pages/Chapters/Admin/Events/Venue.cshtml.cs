using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
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
        var request = await GetAdminServiceRequest();
        var result = await VenueAdminService.UpdateVenue(request, VenueId, new CreateVenue
        {
            Address = viewModel.Address,
            Location = LatLong.FromCoords(viewModel.Lat, viewModel.Long),
            LocationName = viewModel.LocationName,
            Name = viewModel.Name ?? ""
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Venue updated", FeedbackType.Success));
        return Redirect($"/{Chapter.ShortName}/Admin/Events/Venues");
    }
}
