using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Controllers;

[Authorize(Roles = "Admin")]
public class ChapterAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IEmailAdminService _emailAdminService;
    private readonly IEmailService _emailService;
    private readonly IRequestCache _requestCache;

    public ChapterAdminController(IChapterAdminService chapterAdminService, IEmailAdminService emailAdminService,
        IEmailService emailService, IRequestCache requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _emailAdminService = emailAdminService;
        _emailService = emailService;
        _requestCache = requestCache;
    }

    [HttpPost("/{chapterName}/Admin/Chapter/ContactRequests/{id}/Delete")]
    public async Task<IActionResult> DeleteContactRequest(string chapterName, Guid id)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _chapterAdminService.DeleteChapterContactRequest(MemberId, id);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Contact request deleted", FeedbackType.Success));
        }

        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Emails/{type}/RestoreDefault")]
    public async Task<IActionResult> RestoreDefaultEmail(string chapterName, EmailType type)
    {
        var result = await _emailAdminService.DeleteChapterEmail(MemberId, chapterName, type);

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Default email restored", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Emails/{type}/SendTest")]
    public async Task<IActionResult> SendTestEmail(string chapterName, EmailType type)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var member = await _requestCache.GetMemberAsync(MemberId);

        if (member == null)
        {
            return NotFound();
        }

        var result = await _emailService.SendEmail(chapter, member.GetEmailAddressee(), type, new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "member.emailAddress", member.FirstName },
            { "member.firstName", member.FirstName },
            { "member.lastName", member.FirstName }
        });

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Test email sent", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Properties/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        await _chapterAdminService.DeleteChapterProperty(MemberId, id);
        AddFeedback(new FeedbackViewModel("Property deleted", FeedbackType.Success));
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Properties/{id:guid}/MoveDown")]
    public async Task<IActionResult> MovePropertyDown(Guid id)
    {
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(MemberId, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Properties/{id:guid}/MoveUp")]
    public async Task<IActionResult> MovePropertyUp(Guid id)
    {
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(MemberId, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Questions/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        await _chapterAdminService.DeleteChapterQuestion(MemberId, id);
        AddFeedback(new FeedbackViewModel("Question deleted", FeedbackType.Success));
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Questions/{id:guid}/MoveDown")]
    public async Task<IActionResult> MoveQuestionDown(Guid id)
    {
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(MemberId, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Questions/{id:guid}/MoveUp")]
    public async Task<IActionResult> MoveQuestionUp(Guid id)
    {
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(MemberId, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Text")]
    public async Task<IActionResult> UpdateChapterTexts(string chapterName, [FromForm] ChapterTextsFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _chapterAdminService.UpdateChapterTexts(MemberId, chapter.Id, new UpdateChapterTexts
        {
            RegisterText = viewModel.RegisterMessage,
            WelcomeText = viewModel.WelcomeMessage
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }
}
