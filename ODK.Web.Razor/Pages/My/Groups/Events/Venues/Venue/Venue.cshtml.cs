using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services.Security;
using ODK.Services.Venues;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Venues;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Venues.Venue;

public class VenueModel : OdkGroupAdminPageModel
{
    private readonly IVenueAdminService _venueAdminService;

    public VenueModel(IVenueAdminService venueAdminService)
    {
        _venueAdminService = venueAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Venues;

    public Guid VenueId { get; private set; }

    public void OnGet(Guid venueId)
    {
        VenueId = venueId;
    }

    public async Task<IActionResult> OnPostAsync(Guid venueId, VenueFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _venueAdminService.UpdateVenue(request, venueId, new CreateVenue
        {
            Address = viewModel.Address,
            Location = LatLong.FromCoords(viewModel.Lat, viewModel.Long),
            LocationName = viewModel.LocationName,
            Name = viewModel.Name ?? ""
        });

        AddFeedback(result, "Venue updated");

        if (!result.Success)
        {
            return Page();
        }

        var path = OdkRoutes.GroupAdmin.Venues(Chapter).Path;
        return Redirect(path);
    }
}