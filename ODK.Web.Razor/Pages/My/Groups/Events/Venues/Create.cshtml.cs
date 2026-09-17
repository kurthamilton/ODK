using Microsoft.AspNetCore.Mvc;
using ODK.Services.Security;
using ODK.Services.Venues;
using ODK.Services.Venues.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Venues;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Venues;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IVenueAdminService _venueAdminService;

    public CreateModel(IVenueAdminService venueAdminService)
    {
        _venueAdminService = venueAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Venues;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(VenueFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _venueAdminService.CreateVenue(request, new VenueCreateModel
        {
            AdditionalInfo = viewModel.AdditionalInfo,
            ExternalId = viewModel.ExternalId,
            Name = viewModel.Name
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Venue created", FeedbackType.Success);
        var path = OdkRoutes.GroupAdmin.Venues(Chapter).Path;
        return Redirect(path);
    }
}