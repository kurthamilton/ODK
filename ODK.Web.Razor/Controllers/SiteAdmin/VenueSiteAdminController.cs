using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Services.Venues;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Controllers.Admin;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class VenueSiteAdminController : AdminControllerBase
{
    private readonly IVenueSiteAdminService _venueSiteAdminService;

    public VenueSiteAdminController(
        IVenueSiteAdminService venueSiteAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _venueSiteAdminService = venueSiteAdminService;
    }

    [HttpPost("/siteadmin/venues/{id:guid}/backfill")]
    public async Task<IActionResult> Backfill(Guid id, [FromForm] VenueBackfillFormViewModel viewModel)
    {
        var request = MemberServiceRequest;
        var result = await _venueSiteAdminService.BackfillVenue(request, id, viewModel.ExternalId);
        AddFeedback(result, "Venue updated");
        return Redirect(OdkRoutes.SiteAdmin.Venue(id).Path);
    }

    [HttpPost("/siteadmin/venues/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = MemberServiceRequest;
        var result = await _venueSiteAdminService.DeleteVenue(request, id);
        AddFeedback(result, "Venue deleted");
        return RedirectToReferrer();
    }
}
