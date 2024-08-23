using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Subscriptions;
using ODK.Services;
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

    [HttpPost("groups/{id:guid}/privacy")]
    public async Task<IActionResult> UpdatePrivacySettings(Guid id, [FromForm] ChapterPrivacySettingsFormViewModel viewModel)
    {
        var request = new AdminServiceRequest(id, MemberId);
        var result = await _chapterAdminService.UpdateChapterPrivacySettings(request, new UpdateChapterPrivacySettings
        {
            EventResponseVisibility = viewModel.EventResponseVisibility,
            EventVisibility = viewModel.EventVisibility,
            MemberVisibility = viewModel.MemberVisibility,
            VenueVisibility = viewModel.VenueVisibility
        });
        AddFeedback(result, "Privacy settings updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions")]
    public async Task<IActionResult> CreateQuestion(Guid chapterId,
        [FromForm] string? name, [FromForm] string? answer)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        var model = new CreateChapterQuestion
        {
            Answer = answer ?? "",
            Name = name ?? ""
        };
        var result = await _chapterAdminService.CreateChapterQuestion(request, model);
        AddFeedback(result, "Question created");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(Guid chapterId, Guid questionId, 
        [FromForm] string? name, [FromForm] string? answer)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        var model = new CreateChapterQuestion
        {
            Answer = answer ?? "",
            Name = name ?? ""
        };
        var result = await _chapterAdminService.UpdateChapterQuestion(request, 
            questionId, model);
        AddFeedback(result, "Question updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/delete")]
    public async Task<IActionResult> DeleteQuestion(Guid chapterId, Guid questionId)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _chapterAdminService.DeleteChapterQuestion(request, questionId);
        AddFeedback("Question deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/move/down")]
    public async Task<IActionResult> MoveQuestionDown(Guid chapterId, Guid questionId)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(request,
            questionId, 1);
        AddFeedback("Question order updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/move/up")]
    public async Task<IActionResult> MoveQuestionUp(Guid chapterId, Guid questionId)
    {
        var request = new AdminServiceRequest(chapterId, MemberId);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(request,
            questionId, -1);
        AddFeedback("Question order updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("my/groups/{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var request = new AdminServiceRequest(id, MemberId);
        var result = await _chapterAdminService.PublishChapter(request);
        AddFeedback(result, "Group published");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/ContactRequests/{id}/Delete")]
    public async Task<IActionResult> DeleteContactRequest(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.DeleteChapterContactRequest(serviceRequest, id);
        if (result.Success)
        {
            AddFeedback("Contact request deleted", FeedbackType.Success);
        }

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Currency")]
    public async Task<IActionResult> UpdateCurrency(string chapterName, [FromForm] Guid currencyId)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.UpdateChapterCurrency(serviceRequest, currencyId);
        AddFeedback(result, "Currency updated");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Emails/{type}/RestoreDefault")]
    public async Task<IActionResult> RestoreDefaultEmail(string chapterName, EmailType type)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _emailAdminService.DeleteChapterEmail(serviceRequest, type);
        AddFeedback(result, "Default email restored");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Emails/{type}/SendTest")]
    public async Task<IActionResult> SendTestEmail(string chapterName, EmailType type)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _emailAdminService.SendTestEmail(serviceRequest, type);
        AddFeedback(result, "Test email sent");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Owner")]
    public async Task<IActionResult> SetOwner(string chapterName, [FromForm] Guid memberId)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.SetOwner(serviceRequest, memberId);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Properties/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteProperty(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.DeleteChapterProperty(serviceRequest, id);
        AddFeedback("Property deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Properties/{id:guid}/MoveDown")]
    public async Task<IActionResult> MovePropertyDown(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(serviceRequest, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Properties/{id:guid}/MoveUp")]
    public async Task<IActionResult> MovePropertyUp(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(serviceRequest, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Questions/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteQuestion(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.DeleteChapterQuestion(serviceRequest, id);
        AddFeedback("Question deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Questions/{id:guid}/MoveDown")]
    public async Task<IActionResult> MoveQuestionDown(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(serviceRequest, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Questions/{id:guid}/MoveUp")]
    public async Task<IActionResult> MoveQuestionUp(string chapterName, Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(serviceRequest, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Subscriptions/{id:guid}/Automatic")]
    public async Task<IActionResult> SetUpAutomaticSubscription(string chapterName, Guid id, 
        [FromForm] SiteSubscriptionFrequency frequency)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.UpdateChapterSiteSubscription(serviceRequest, id, frequency);
        AddFeedback(result, "Automatic subscription set up");
        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Admin/Chapter/Text")]
    public async Task<IActionResult> UpdateChapterTexts(string chapterName,
        [FromForm] ChapterTextsFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest(chapterName);
        var result = await _chapterAdminService.UpdateChapterTexts(serviceRequest, new UpdateChapterTexts
        {
            Description = viewModel.Description,
            RegisterText = viewModel.RegisterMessage,
            WelcomeText = viewModel.WelcomeMessage
        });

        AddFeedback(result, "Texts updated");

        return RedirectToReferrer();
    }
}
