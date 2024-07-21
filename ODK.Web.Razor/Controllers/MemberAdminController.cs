using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Services.Members;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Controllers;

[Authorize(Roles = "Admin")]
public class MemberAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IEmailService _emailService;
    private readonly IMemberAdminService _memberAdminService;
    private readonly IRequestCache _requestCache;

    public MemberAdminController(IMemberAdminService memberAdminService, IEmailService emailService,
        IChapterAdminService chapterAdminService,
        IRequestCache requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _emailService = emailService;
        _memberAdminService = memberAdminService;
        _requestCache = requestCache;
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteMember(string chapterName, Guid id)
    {
        await _memberAdminService.DeleteMember(MemberId, id);

        AddFeedback(new FeedbackViewModel("Member deleted", FeedbackType.Success));

        return Redirect($"/{chapterName}/Admin/Members");
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/Picture/Rotate")]
    public async Task<IActionResult> RotatePicture(Guid id)
    {
        await _memberAdminService.RotateMemberImage(MemberId, id, 90);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/ResendActivationEmail")]
    public async Task<IActionResult> ResendActivationEmail(Guid id)
    {
        await _memberAdminService.SendActivationEmail(MemberId, id);
        AddFeedback(new FeedbackViewModel("Email sent", FeedbackType.Success));
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/{id:guid}/SendEmail")]
    public async Task<IActionResult> SendEmail(Guid id, [FromForm] SendMemberEmailFormViewModel viewModel)
    {
        ServiceResult result = await _emailService.SendMemberEmail(MemberId, id, 
            viewModel.Subject, viewModel.Body);

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Email sent", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/AdminMembers/Add")]
    public async Task<IActionResult> AddAdminMember(string chapterName, [FromForm] AdminMemberAddFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _chapterAdminService.AddChapterAdminMember(MemberId, chapter.Id, 
            viewModel.MemberId!.Value);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Admin member added", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/AdminMembers/{id:guid}/Delete")]
    public async Task<IActionResult> AddAdminMember(string chapterName, Guid id)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _chapterAdminService.DeleteChapterAdminMember(MemberId, chapter.Id, id);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Admin member removed", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpGet("{chapterName}/Admin/Members/Download")]
    public async Task<IActionResult> DownloadAdminMembers(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var data = await _memberAdminService.GetMemberCsv(MemberId, chapter.Id);
        
        return DownloadCsv(data, $"Members.{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost("{chapterName}/Admin/Members/SendEmail")]
    public async Task<IActionResult> SendBulkEmail(string chapterName, [FromForm] SendMemberBulkEmailFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);

        var members = await _memberAdminService.GetMembers(MemberId, new MemberFilter
        {
            ChapterId = chapter.Id,
            Statuses = viewModel.Status,
            Types = viewModel.Type
        });

        await _emailService.SendBulkEmail(MemberId, chapter, members, viewModel.Subject, viewModel.Body);

        AddFeedback(new FeedbackViewModel($"Bulk email sent to {members.Count} members", FeedbackType.Success));

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Members/Subscriptions/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSubscription(Guid id)
    {
        ServiceResult result = await _chapterAdminService.DeleteChapterSubscription(MemberId, id);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Subscription deleted", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }
}
