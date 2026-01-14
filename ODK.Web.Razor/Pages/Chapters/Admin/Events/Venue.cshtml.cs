using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services.Venues;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenueModel : VenueAdminPageModel
{
    public VenueModel(IVenueAdminService venueAdminService)
        : base(venueAdminService)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(VenueFormViewModel viewModel)
    {
        var request = await CreateMemberChapterServiceRequest();
        var result = await VenueAdminService.UpdateVenue(request, Venue.Id, new CreateVenue
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

        var chapter = await GetChapter();
        AddFeedback(new FeedbackViewModel("Venue updated", FeedbackType.Success));
        return Redirect($"/{chapter.ShortName}/Admin/Events/Venues");
    }
}