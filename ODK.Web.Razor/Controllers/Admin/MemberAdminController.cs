using Microsoft.AspNetCore.Mvc;
using ODK.Core.Images;
using ODK.Services;
using ODK.Services.Csv;
using ODK.Services.Chapters;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Members;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

public class MemberAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly ICsvWriter _csvWriter;
    private readonly IMemberAdminService _memberAdminService;
    private readonly IMemberImportFileReader _memberImportFileReader;
    private readonly IMemberImportService _memberImportService;

    public MemberAdminController(
        IMemberAdminService memberAdminService,
        IChapterAdminService chapterAdminService,
        ICsvWriter csvWriter,
        IMemberImportFileReader memberImportFileReader,
        IMemberImportService memberImportService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterAdminService = chapterAdminService;
        _csvWriter = csvWriter;
        _memberAdminService = memberAdminService;
        _memberImportFileReader = memberImportFileReader;
        _memberImportService = memberImportService;
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/approve")]
    public async Task<IActionResult> ApproveMember(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberApprovals, MemberChapterServiceRequest);
        var result = await _memberAdminService.ApproveMember(request, id);
        AddFeedback(result, "Member approved");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/picture")]
    public async Task<IActionResult> UpdatePicture(Guid chapterId, Guid id,
        [FromForm] string imageDataUrl)
    {
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

        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImage, MemberChapterServiceRequest);
        var result = await _memberAdminService.UpdateMemberImage(request, id, new MemberImageUpdateModel
        {
            ImageData = bytes
        });
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/delete")]
    public async Task<IActionResult> DeleteMember(Guid chapterId, Guid id, [FromForm] string? reason)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberApprovals, MemberChapterServiceRequest);
        var result = await _memberAdminService.RemoveMemberFromChapter(request, id, reason);
        AddFeedback(result, "Member deleted");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        return Redirect(OdkRoutes.GroupAdmin.Members(Chapter).Path);
    }

    [HttpPost("groups/{chapterId:guid}members/{id:guid}/emails/activation/send")]
    public async Task<IActionResult> SendActivationEmail(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Members, MemberChapterServiceRequest);
        await _memberAdminService.SendActivationEmail(request, id);
        AddFeedback("Email sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/{id:guid}/visibility")]
    public async Task<IActionResult> SetMemberVisibility(Guid chapterId, Guid id, [FromForm] bool visible)
    {
        var request = MemberChapterServiceRequest;
        await _memberAdminService.SetMemberVisibility(request, id, visible);
        AddFeedback("Member updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/admins")]
    public async Task<IActionResult> AddAdminMember(Guid chapterId,
        [FromForm] AdminMemberAddFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.AdminMembers, MemberChapterServiceRequest);
        var result = await _chapterAdminService.AddChapterAdminMember(request, viewModel.MemberId!.Value);
        AddFeedback(result, "Admin member added");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/admins/{memberId:guid}/delete")]
    public async Task<IActionResult> AddAdminMember(Guid chapterId, Guid memberId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.AdminMembers, MemberChapterServiceRequest);
        var result = await _chapterAdminService.DeleteChapterAdminMember(request, memberId);
        AddFeedback(result, "Admin member removed");
        return RedirectToReferrer();
    }

    [HttpGet("groups/{chapterId:guid}/members/download")]
    public async Task<IActionResult> DownloadAdminMembers(Guid chapterId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberExport, MemberChapterServiceRequest);
        var data = await _memberAdminService.GetMemberCsv(request);

        return DownloadCsv(data, $"Members.{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost("groups/{chapterId:guid}/members/email")]
    public async Task<IActionResult> SendBulkEmail(Guid chapterId,
        [FromForm] SendMemberBulkEmailFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.BulkEmail, MemberChapterServiceRequest);
        var result = await _memberAdminService.SendBulkEmail(
            request, viewModel.MemberIds, viewModel.Subject, viewModel.BodyHtml);
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpGet("groups/{chapterId:guid}/members/import/template")]
    public async Task<IActionResult> DownloadMemberImportTemplate(Guid chapterId)
    {
        var data = await _memberImportService.GetMemberImportTemplate(MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImport, MemberChapterServiceRequest));

        return DownloadCsv(data, "member-import.csv");
    }

    [HttpPost("groups/{chapterId:guid}/members/import")]
    public async Task<IActionResult> UploadMemberImport(
        Guid chapterId,
        [FromForm] MemberImportUploadSubmitViewModel viewModel)
    {
        var read = _memberImportFileReader.Read(viewModel.File);
        if (!read.Success || read.Value == null)
        {
            AddFeedback(read);
            return RedirectToReferrer();
        }

        var result = await _memberImportService.StageMembers(
            MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImport, MemberChapterServiceRequest), read.Value, viewModel.File?.FileName);

        AddFeedback(result, UploadMessage(result));

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/import/invite")]
    public async Task<IActionResult> InviteStagedMembers(Guid chapterId)
    {
        var result = await _memberAdminService.InviteStagedMembers(MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImport, MemberChapterServiceRequest));

        AddFeedback(result, "Invites sent");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        return Redirect(OdkRoutes.GroupAdmin.MembersInvited(Chapter).Path);
    }

    [HttpPost("groups/{chapterId:guid}/members/import/{id:guid}/delete")]
    public async Task<IActionResult> DeleteStagedMember(Guid chapterId, Guid id)
    {
        var result = await _memberImportService.DeleteStagedMember(MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImport, MemberChapterServiceRequest), id);

        AddFeedback(result, "Person removed");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/import/clear")]
    public async Task<IActionResult> ClearStagedMembers(Guid chapterId)
    {
        var result = await _memberImportService.ClearStagedMembers(MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImport, MemberChapterServiceRequest));

        AddFeedback(result, "Everyone waiting to be invited was removed");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/members/subscriptions/{id:guid}/delete")]
    public async Task<IActionResult> DeleteSubscription(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Subscriptions, MemberChapterServiceRequest);
        var result = await _chapterAdminService.DeleteChapterSubscription(request, id);
        AddFeedback(result, "Subscription deleted");
        return RedirectToReferrer();
    }

    private static string UploadMessage(StageMemberImportResult result)
    {
        var parts = new List<string>();

        if (result.Staged > 0)
        {
            parts.Add($"{result.Staged} added");
        }

        if (result.Updated > 0)
        {
            parts.Add($"{result.Updated} updated");
        }

        if (result.AlreadyInGroup > 0)
        {
            parts.Add($"{result.AlreadyInGroup} already in the group");
        }

        if (result.AlreadyInvited > 0)
        {
            parts.Add($"{result.AlreadyInvited} already invited");
        }

        // Nothing to report only when the file held people the group already has or has already asked.
        return parts.Count > 0
            ? string.Join(", ", parts)
            : "There was nobody new in that file";
    }

    private IActionResult DownloadCsv(IReadOnlyCollection<IReadOnlyCollection<string>> data, string fileName)
        => File(_csvWriter.Write(data), "text/csv", fileName);
}