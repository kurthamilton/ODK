using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Security;
using ODK.Services.Venues;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers.Admin;

public class VenueAdminController : AdminControllerBase
{
    private readonly IVenueAdminService _venueAdminService;

    public VenueAdminController(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes,
        IVenueAdminService venueAdminService)
        : base(requestStore, odkRoutes)
    {
        _venueAdminService = venueAdminService;
    }

    [HttpPost("groups/{chapterId:guid}/venues/{id:guid}/archive")]
    public async Task<IActionResult> ArchiveVenue(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Venues,
            MemberChapterServiceRequest);
        var result = await _venueAdminService.ArchiveVenue(request, id);
        AddFeedback(result, "Venue archived");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/venues/{id:guid}/restore")]
    public async Task<IActionResult> RestoreVenue(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Venues,
            MemberChapterServiceRequest);
        var result = await _venueAdminService.RestoreVenue(request, id);
        AddFeedback(result, "Venue restored");
        return RedirectToReferrer();
    }
}