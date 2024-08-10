using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Controllers.Admin;

public class ChapterAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IEmailAdminService _emailAdminService;

    public ChapterAdminController(
        IChapterAdminService chapterAdminService,
        IEmailAdminService emailAdminService,
        IRequestCache requestCache)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
        _emailAdminService = emailAdminService;
    }

    [HttpPost("/{chapterName}/Admin/Chapter/ContactRequests/{id}/Delete")]
    public async Task<IActionResult> DeleteContactRequest(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.DeleteChapterContactRequest(serviceRequest, id);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Contact request deleted", FeedbackType.Success));
        }

        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Emails/{type}/RestoreDefault")]
    public async Task<IActionResult> RestoreDefaultEmail(string chapterName, EmailType type)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _emailAdminService.DeleteChapterEmail(serviceRequest, type);

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
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _emailAdminService.SendTestEmail(serviceRequest, type);

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

    [HttpPost("/{chapterName}/Admin/Chapter/Owner")]
    public async Task<IActionResult> SetOwner(string chapterName, [FromForm] Guid memberId)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.SetOwner(serviceRequest, memberId);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Properties/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteProperty(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.DeleteChapterProperty(serviceRequest, id);
        AddFeedback(new FeedbackViewModel("Property deleted", FeedbackType.Success));
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Properties/{id:guid}/MoveDown")]
    public async Task<IActionResult> MovePropertyDown(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(serviceRequest, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Properties/{id:guid}/MoveUp")]
    public async Task<IActionResult> MovePropertyUp(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(serviceRequest, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Questions/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteQuestion(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.DeleteChapterQuestion(serviceRequest, id);
        AddFeedback(new FeedbackViewModel("Question deleted", FeedbackType.Success));
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Questions/{id:guid}/MoveDown")]
    public async Task<IActionResult> MoveQuestionDown(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(serviceRequest, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Questions/{id:guid}/MoveUp")]
    public async Task<IActionResult> MoveQuestionUp(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(serviceRequest, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("/{chapterName}/Admin/Chapter/Text")]
    public async Task<IActionResult> UpdateChapterTexts(string chapterName,
        [FromForm] ChapterTextsFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.UpdateChapterTexts(serviceRequest, new UpdateChapterTexts
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
