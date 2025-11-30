using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services.Chapters;
using ODK.Services.Venues;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Venues;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Venues.Venue;

public class VenueModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IVenueAdminService _venueAdminService;

    public VenueModel(IVenueAdminService venueAdminService,
        IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
        _venueAdminService = venueAdminService;
    }

    public Guid VenueId { get; private set; }

    public void OnGet(Guid venueId)
    {
        VenueId = venueId;
    }

    public async Task<IActionResult> OnPostAsync(Guid venueId, VenueFormViewModel viewModel)
    {
        var result = await _venueAdminService.UpdateVenue(AdminServiceRequest, venueId, new CreateVenue
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

        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);
        return Redirect(OdkRoutes.MemberGroups.Venues(Platform, chapter));
    }
}
