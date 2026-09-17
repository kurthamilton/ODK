using Microsoft.AspNetCore.Mvc;
using ODK.Services.Venues;
using ODK.Services.Venues.Models;
using ODK.Web.Razor.Models.Admin.Venues;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Venues;

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
        var request = MemberChapterAdminServiceRequest;
        var result = await VenueAdminService.UpdateVenue(request, Venue.Id, new VenueUpdateModel
        {
            AdditionalInfo = viewModel.AdditionalInfo,
            Name = viewModel.Name
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Venue updated", FeedbackType.Success);
        return Redirect(OdkRoutes.GroupAdmin.Venues(Chapter).Path);
    }
}