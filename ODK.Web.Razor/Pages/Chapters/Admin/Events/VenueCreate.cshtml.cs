using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
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
        var request = await GetAdminServiceRequest();
        var result = await _venueAdminService.CreateVenue(request, new CreateVenue
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

        AddFeedback(new FeedbackViewModel("Venue created", FeedbackType.Success));
        return Redirect($"/{Chapter.ShortName}/Admin/Events/Venues");
    }
}
