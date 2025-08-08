using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Controllers.Admin;
using ODK.Web.Razor.Models.Chapters.SuperAdmin;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

[Authorize(Roles = OdkRoles.SuperAdmin)]
public class ChapterSuperAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPaymentAdminService _paymentAdminService;

    public ChapterSuperAdminController(
        IChapterAdminService chapterAdminService, 
        IRequestCache requestCache,
        IPaymentAdminService paymentAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _paymentAdminService = paymentAdminService;
    }

    [HttpPost("/superadmin/groups/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var request = new AdminServiceRequest(id, MemberId);
        var result = await _chapterAdminService.ApproveChapter(request);
        AddFeedback(result, "Group approved");
        return RedirectToReferrer();
    }

    [HttpPost("/superadmin/groups/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = new AdminServiceRequest(id, MemberId);
        var result = await _chapterAdminService.DeleteChapter(request);
        AddFeedback(result, "Chapter deleted");
        return RedirectToReferrer();
    }

    [HttpPost("/superadmin/groups/{groupId:guid}/emails/providers/{id:guid}/delete")]
    public async Task<IActionResult> DeleteEmailProvider(Guid groupId, Guid id)
    {
        var request = new AdminServiceRequest(groupId, MemberId);
        await _chapterAdminService.DeleteChapterEmailProvider(request, id);
        AddFeedback("Email provider deleted", FeedbackType.Success);
        return RedirectToReferrer();
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

    [HttpPost("/{chapterName}/Admin/SuperAdmin/Payments/{id:guid}/Reconciliation-Status")]
    public async Task<IActionResult> AddReconciliationExemption(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);

        await _paymentAdminService.SetPaymentReconciliationExemption(request, id, true);

        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/SuperAdmin/Payments/Reconciliations")]
    public async Task<IActionResult> CreateReconciliation(string chapterName, ReconciliationFormViewModel viewModel)
    {
        var request = await GetAdminServiceRequest(chapterName);

        var model = new CreateReconciliationModel
        {
            PaymentIds = viewModel.PaymentIds,
            PaymentReference = viewModel.PaymentReference
        };

        await _paymentAdminService.CreateReconciliation(request, model);

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
