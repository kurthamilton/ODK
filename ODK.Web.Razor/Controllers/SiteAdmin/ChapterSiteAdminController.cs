using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Controllers.Admin;
using ODK.Web.Razor.Models.Chapters.SiteAdmin;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class ChapterSiteAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IChapterSiteAdminService _chapterSiteAdminService;

    public ChapterSiteAdminController(
        IChapterAdminService chapterAdminService,
        IChapterSiteAdminService chapterSiteAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterAdminService = chapterAdminService;
        _chapterSiteAdminService = chapterSiteAdminService;
    }

    [HttpPost("/siteadmin/groups/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var request = MemberServiceRequest;
        var result = await _chapterSiteAdminService.ApproveChapter(request, id);
        AddFeedback(result, "Group approved");
        return RedirectToReferrer();
    }

    [HttpPost("/siteadmin/groups/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = MemberServiceRequest;
        var result = await _chapterSiteAdminService.DeleteChapter(request, id);
        AddFeedback(result, "Chapter deleted");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/siteadmin/payments")]
    public async Task<IActionResult> UpdatePaymentSettings(
        Guid chapterId, [FromForm] PaymentSettingsFormSubmitViewModel viewModel)
    {
        var request = AdminRequest();
        var result = await _chapterAdminService.UpdateChapterPaymentSettings(request,
            new ChapterPaymentSettingsUpdateModel
            {
                CurrencyId = viewModel.CurrencyId
            });

        AddFeedback(result, "Payment settings updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/siteadmin/redirect")]
    public async Task<IActionResult> UpdateRedirectUrl(Guid chapterId, [FromForm] string? redirectUrl)
    {
        var request = AdminRequest();
        await _chapterAdminService.UpdateChapterRedirectUrl(request, redirectUrl);
        AddFeedback("Redirect updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    /// <summary>
    /// Site admin is the gate on this controller, so the securable only has to name a group role the
    /// request can be built with. None of these actions is delegable to a group admin.
    /// </summary>
    private IMemberChapterAdminServiceRequest AdminRequest() => MemberChapterAdminServiceRequest.Create(
        ChapterAdminSecurable.Any, MemberChapterServiceRequest);
}
