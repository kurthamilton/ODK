using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Chapters;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Web.Razor.Controllers.Admin;
using ODK.Web.Razor.Models.Chapters.SuperAdmin;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

[Authorize(Roles = OdkRoles.SuperAdmin)]
public class ChapterSuperAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPaymentAdminService _paymentAdminService;

    public ChapterSuperAdminController(
        IChapterAdminService chapterAdminService,
        IPaymentAdminService paymentAdminService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _chapterAdminService = chapterAdminService;
        _paymentAdminService = paymentAdminService;
    }

    [HttpPost("/superadmin/groups/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var request = CreateMemberChapterServiceRequest(id);
        var result = await _chapterAdminService.ApproveChapter(request);
        AddFeedback(result, "Group approved");
        return RedirectToReferrer();
    }

    [HttpPost("/superadmin/groups/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = CreateMemberChapterServiceRequest(id);
        var result = await _chapterAdminService.DeleteChapter(request);
        AddFeedback(result, "Chapter deleted");
        return RedirectToReferrer();
    }

    [HttpPost("/groups/{chapterId:guid}/SuperAdmin/Payments/{id:guid}/Reconciliation-Status")]
    public async Task<IActionResult> AddReconciliationExemption(Guid chapterId, Guid id)
    {
        var request = MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);

        await _paymentAdminService.SetPaymentReconciliationExemption(request, id, true);

        return RedirectToReferrer();
    }

    [HttpPost("/groups/{chapterId:guid}/SuperAdmin/Payments/Reconciliations")]
    public async Task<IActionResult> CreateReconciliation(Guid chapterId, ReconciliationFormViewModel viewModel)
    {
        var request = MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);

        var model = new CreateReconciliationModel
        {
            PaymentIds = viewModel.PaymentIds,
            PaymentReference = viewModel.PaymentReference
        };

        await _paymentAdminService.CreateReconciliation(request, model);

        return RedirectToReferrer();
    }
}