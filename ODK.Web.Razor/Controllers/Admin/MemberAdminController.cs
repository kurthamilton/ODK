using Microsoft.AspNetCore.Mvc;
using ODK.Core.Images;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Members;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Controllers.Admin;

public class MemberAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IMemberAdminService _memberAdminService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IRequestCache _requestCache;

    public MemberAdminController(
        IMemberAdminService memberAdminService, 
        IChapterAdminService chapterAdminService,
        IRequestCache requestCache,
        IPlatformProvider platformProvider)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _memberAdminService = memberAdminService;
        _platformProvider = platformProvider;
        _requestCache = requestCache;
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/picture")]
    public async Task<IActionResult> UpdatePicture(Guid chapterId, Guid id,
        [FromForm] string imageDataUrl)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        
        if (string.IsNullOrEmpty(imageDataUrl))
        {
            AddFeedback("No image provided", FeedbackType.Warning);
            return RedirectToReferrer();
        }

        if (!ImageHelper.TryParseDataUrl(imageDataUrl, out var bytes))
        {
            AddFeedback("Image could not be processed", FeedbackType.Error);
            return RedirectToReferrer();
        }

        var result = await _memberAdminService.UpdateMemberImage(request, id, new UpdateMemberImage
        { 
            ImageData = bytes
        });
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/delete")]
    public async Task<IActionResult> DeleteMember(Guid chapterId, Guid id, [FromForm] string? reason)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        var result = await _memberAdminService.RemoveMemberFromChapter(request, id, reason);
        AddFeedback(result, "Member deleted");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterAdminService.GetChapter(request);
        return Redirect(OdkRoutes.MemberGroups.Members(platform, chapter));
    }

    [HttpPost("groups/{chapterId:guid}members/{id:guid}/emails/activation/send")]
    public async Task<IActionResult> SendActivationEmail(Guid chapterId, Guid id)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _memberAdminService.SendActivationEmail(request, id);
        AddFeedback("Email sent", FeedbackType.Success);
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

    [HttpPost("groups/{id:guid}/members/admins")]
    public async Task<IActionResult> AddAdminMember(Guid id, 
        [FromForm] AdminMemberAddFormViewModel viewModel)
    {
        var serviceRequest = new AdminServiceRequest(id, MemberId);
        var result = await _chapterAdminService.AddChapterAdminMember(serviceRequest, viewModel.MemberId!.Value);
        AddFeedback(result, "Admin member added");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{id:guid}/members/admins/{memberId:guid}/delete")]
    public async Task<IActionResult> AddAdminMember(Guid id, Guid memberId)
    {
        var serviceRequest = new AdminServiceRequest(id, MemberId);
        var result = await _chapterAdminService.DeleteChapterAdminMember(serviceRequest, memberId);
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

    [HttpPost("groups/{chapterId:guid}/members/email")]
    public async Task<IActionResult> SendBulkEmail(Guid chapterId, 
        [FromForm] SendMemberBulkEmailFormViewModel viewModel)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);

        var filter = new MemberFilter
        {
            Statuses = viewModel.Status,
            Types = viewModel.Type
        };

        var result = await _memberAdminService.SendBulkEmail(request, filter, viewModel.Subject, viewModel.Body);
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/subscriptions/{id:guid}/delete")]
    public async Task<IActionResult> DeleteSubscription(Guid chapterId, Guid id)
    {
        var serviceRequest = new AdminServiceRequest(chapterId, MemberId);
        var result = await _chapterAdminService.DeleteChapterSubscription(serviceRequest, id);
        AddFeedback(result, "Subscription deleted");
        return RedirectToReferrer();
    }
}
