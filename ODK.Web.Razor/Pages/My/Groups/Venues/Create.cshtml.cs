using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Venues;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Venues;

namespace ODK.Web.Razor.Pages.My.Groups.Venues;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterService _chapterService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IVenueAdminService _venueAdminService;

    public CreateModel(
        IVenueAdminService venueAdminService,
        IChapterService chapterService,
        IPlatformProvider platformProvider) 
    {
        _chapterService = chapterService;
        _platformProvider = platformProvider;
        _venueAdminService = venueAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(VenueFormViewModel viewModel)
    {
        var result = await _venueAdminService.CreateVenue(AdminServiceRequest, new CreateVenue
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

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterService.GetChapterById(ChapterId);
        AddFeedback(new FeedbackViewModel("Venue created", FeedbackType.Success));
        var url = OdkRoutes2.MemberGroups.Venues(platform, chapter);
        return Redirect(url);
    }
}
