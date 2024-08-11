using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Members;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Controllers.Admin;

public class MemberAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IMemberAdminService _memberAdminService;
    private readonly IRequestCache _requestCache;

    public MemberAdminController(
        IMemberAdminService memberAdminService, 
        IChapterAdminService chapterAdminService,
        IRequestCache requestCache)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _memberAdminService = memberAdminService;
        _requestCache = requestCache;
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteMember(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _memberAdminService.DeleteMember(request, id);
        AddFeedback("Member deleted", FeedbackType.Success);
        return Redirect($"/{chapterName}/Admin/Members");
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/Picture/Rotate")]
    public async Task<IActionResult> RotatePicture(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _memberAdminService.RotateMemberImage(request, id);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/ResendActivationEmail")]
    public async Task<IActionResult> ResendActivationEmail(string chapterName, Guid id)
    {
        var request = await GetAdminServiceRequest(chapterName);
        await _memberAdminService.SendActivationEmail(request, id);
        AddFeedback("Email sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/SendEmail")]
    public async Task<IActionResult> SendEmail(string chapterName, Guid id, [FromForm] SendMemberEmailFormViewModel viewModel)
    {
        var request = await GetAdminServiceRequest(chapterName);
        var result = await _memberAdminService.SendMemberEmail(request, id, viewModel.Subject, viewModel.Body);
        AddFeedback(result, "Email sent");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/Visibility")]
    public async Task<IActionResult> SetMemberVisibility(string chapterName, Guid id, [FromForm] bool visible)
    {
        var request = await GetAdminServiceRequest(chapterName); 
        await _memberAdminService.SetMemberVisibility(request, id, visible);
        AddFeedback("Member updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/AdminMembers/Add")]
    public async Task<IActionResult> AddAdminMember(string chapterName, 
        [FromForm] AdminMemberAddFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.AddChapterAdminMember(serviceRequest, viewModel.MemberId!.Value);
        AddFeedback(result, "Admin member added");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/AdminMembers/{id:guid}/Delete")]
    public async Task<IActionResult> AddAdminMember(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.DeleteChapterAdminMember(serviceRequest, id);
        AddFeedback(result, "Admin member removed");
        return RedirectToReferrer();
    }

    [HttpGet("{chapterName}/Admin/Members/Download")]
    public async Task<IActionResult> DownloadAdminMembers(string chapterName)
    {
        var request = await GetAdminServiceRequest(chapterName);
        var data = await _memberAdminService.GetMemberCsv(request);

        return DownloadCsv(data, $"Members.{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost("{chapterName}/Admin/Members/SendEmail")]
    public async Task<IActionResult> SendBulkEmail(string chapterName, [FromForm] SendMemberBulkEmailFormViewModel viewModel)
    {
        var request = await GetAdminServiceRequest(chapterName);

        var filter = new MemberFilter
        {
            Statuses = viewModel.Status,
            Types = viewModel.Type
        };

        var result = await _memberAdminService.SendBulkEmail(request, filter, viewModel.Subject, viewModel.Body);
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/Subscriptions/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSubscription(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.DeleteChapterSubscription(serviceRequest, id);
        AddFeedback(result, "Subscription deleted");
        return RedirectToReferrer();
    }
}
