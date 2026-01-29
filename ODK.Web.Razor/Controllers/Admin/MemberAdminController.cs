using Microsoft.AspNetCore.Mvc;
using ODK.Core.Images;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Controllers.Admin;

public class MemberAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IMemberAdminService _memberAdminService;

    public MemberAdminController(
        IMemberAdminService memberAdminService,
        IChapterAdminService chapterAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterAdminService = chapterAdminService;
        _memberAdminService = memberAdminService;
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/approve")]
    public async Task<IActionResult> ApproveMember(Guid chapterId, Guid id)
    {
        var request = MemberChapterServiceRequest;
        var result = await _memberAdminService.ApproveMember(request, id);
        AddFeedback(result, "Member approved");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/picture")]
    public async Task<IActionResult> UpdatePicture(Guid chapterId, Guid id,
        [FromForm] string imageDataUrl)
    {
        var request = MemberChapterServiceRequest;

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
        var request = MemberChapterServiceRequest;
        var result = await _memberAdminService.RemoveMemberFromChapter(request, id, reason);
        AddFeedback(result, "Member deleted");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        var chapter = Chapter;
        return Redirect(OdkRoutes.GroupAdmin.Members(Chapter));
    }

    [HttpPost("groups/{chapterId:guid}members/{id:guid}/emails/activation/send")]
    public async Task<IActionResult> SendActivationEmail(Guid chapterId, Guid id)
    {
        var request = MemberChapterServiceRequest;
        await _memberAdminService.SendActivationEmail(request, id);
        AddFeedback("Email sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/visibility")]
    public async Task<IActionResult> SetMemberVisibility(Guid chapterId, Guid id, [FromForm] bool visible)
    {
        var request = MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);
        await _memberAdminService.SetMemberVisibility(request, id, visible);
        AddFeedback("Member updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/admins")]
    public async Task<IActionResult> AddAdminMember(Guid chapterId,
        [FromForm] AdminMemberAddFormViewModel viewModel)
    {
        var request = MemberChapterServiceRequest;
        var result = await _chapterAdminService.AddChapterAdminMember(request, viewModel.MemberId!.Value);
        AddFeedback(result, "Admin member added");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/admins/{memberId:guid}/delete")]
    public async Task<IActionResult> AddAdminMember(Guid chapterId, Guid memberId)
    {
        var request = MemberChapterServiceRequest;
        var result = await _chapterAdminService.DeleteChapterAdminMember(request, memberId);
        AddFeedback(result, "Admin member removed");
        return RedirectToReferrer();
    }

    [HttpGet("groups/{chapterId:guid}/members/download")]
    public async Task<IActionResult> DownloadAdminMembers(Guid chapterId)
    {
        var request = MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);
        var data = await _memberAdminService.GetMemberCsv(request);

        return DownloadCsv(data, $"Members.{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost("groups/{chapterId:guid}/members/email")]
    public async Task<IActionResult> SendBulkEmail(Guid chapterId,
        [FromForm] SendMemberBulkEmailFormViewModel viewModel)
    {
        var request = MemberChapterServiceRequest;

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
        var request = MemberChapterServiceRequest;
        var result = await _chapterAdminService.DeleteChapterSubscription(request, id);
        AddFeedback(result, "Subscription deleted");
        return RedirectToReferrer();
    }
}