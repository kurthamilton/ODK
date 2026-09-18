using Microsoft.AspNetCore.Mvc;
using ODK.Services.Security;
using ODK.Services.Venues;
using ODK.Services.Venues.Models;
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

    public Guid ChapterVenueId { get; private set; }

    public void OnGet(Guid chapterVenueId)
    {
        ChapterVenueId = chapterVenueId;
    }

    public async Task<IActionResult> OnPostAsync(Guid venueId, VenueFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _venueAdminService.UpdateVenue(request, venueId, new VenueUpdateModel
        {
            AdditionalInfo = viewModel.AdditionalInfo,
            Name = viewModel.Name
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