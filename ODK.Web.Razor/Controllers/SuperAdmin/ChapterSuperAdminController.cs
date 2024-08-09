using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Controllers.Admin;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

[Authorize(Roles = OdkRoles.SuperAdmin)]
public class ChapterSuperAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;

    public ChapterSuperAdminController(IChapterAdminService chapterAdminService, IRequestCache requestCache)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    [HttpPost("/{chapterName}/Admin/SuperAdmin/Location")]
    public async Task<IActionResult> UpdateLocation(string chapterName, ChapterLocationFormViewModel viewModel)
    {
        var lat = viewModel.Lat;
        var lng = viewModel.Long;

        var location = lat != null && lng != null
            ? new LatLong(lat.Value, lng.Value)
            : default(LatLong?);

        var request = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterLocation(request, location, viewModel.Name);

        AddFeedback("Location updated", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/SuperAdmin/TimeZone")]
    public async Task<IActionResult> UpdateTimeZone(string chapterName, ChapterTimeZoneFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.UpdateChapterTimeZone(serviceRequest, viewModel.TimeZone);
        if (result.Success)
        {
            AddFeedback("Time zone updated", FeedbackType.Success);
            return RedirectToReferrer();
        }

        AddFeedback(result);
        return RedirectToReferrer();
    }
}
