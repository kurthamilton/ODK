using Microsoft.AspNetCore.Mvc;
using ODK.Services.Security;
using ODK.Services.Venues;
using ODK.Services.Venues.Models;
using ODK.Web.Razor.Models.Admin.Venues;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Venues;

public class CreateModel : AdminPageModel
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
        return Redirect(AdminRoutes.Venues(Chapter).Path);
    }
}