using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Core.Images;
using ODK.Core.Subscriptions;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Admin.Members;
using ODK.Web.Razor.Models.Chapters.SuperAdmin;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

public class ChapterAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IEmailAdminService _emailAdminService;

    public ChapterAdminController(
        IChapterAdminService chapterAdminService,
        IEmailAdminService emailAdminService,
        IRequestCache requestCache,
        IRequestStore requestStore)
        : base(requestCache, requestStore)
    {
        _chapterAdminService = chapterAdminService;
        _emailAdminService = emailAdminService;
    }

    [HttpPost("groups/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        var result = await _chapterAdminService.DeleteChapter(request);
        AddFeedback(result, "Group deleted");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        return Redirect(OdkRoutes.MemberGroups.Index(Platform));
    }

    [HttpPost("groups/{id:guid}/image")]
    public async Task<IActionResult> UpdateImage(Guid id, [FromForm] ChapterImageFormViewModel viewModel)
    {
        if (string.IsNullOrEmpty(viewModel.ImageDataUrl))
        {
            AddFeedback("No image provided", FeedbackType.Warning);
            return RedirectToReferrer();
        }

        if (!ImageHelper.TryParseDataUrl(viewModel.ImageDataUrl, out var bytes))
        {
            AddFeedback("Image could not be processed", FeedbackType.Error);
            return RedirectToReferrer();
        }

        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        var result = await _chapterAdminService.UpdateChapterImage(request, new UpdateChapterImage
        {
            ImageData = bytes
        });

        AddFeedback(result, "Image updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{id:guid}/links")]
    public async Task<IActionResult> UpdatePrivacySettings(Guid id, [FromForm] ChapterLinksFormSubmitViewModel viewModel)
    {
        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        await _chapterAdminService.UpdateChapterLinks(request, new UpdateChapterLinks
        {
            Facebook = viewModel.Facebook ?? "",
            Instagram = viewModel.Instagram ?? "",
            InstagramFeed = viewModel.ShowInstagramFeed,
            Twitter = viewModel.Twitter ?? ""
        });

        AddFeedback("Social media links updated", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("groups/{id:guid}/membership")]
    public async Task<IActionResult> UpdateMembershipSettings(Guid id, 
        [FromForm] MembershipSettingsFormViewModel viewModel)
    {
        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        var result = await _chapterAdminService.UpdateChapterMembershipSettings(request,
            new UpdateChapterMembershipSettings
            {
                ApproveNewMembers = viewModel.ApproveNewMembers,
                Enabled = viewModel.Enabled,
                MembershipDisabledAfterDaysExpired = viewModel.MembershipDisabledAfter,
                MembershipExpiringWarningDays = viewModel.MembershipExpiringWarningDays,
                TrialPeriodMonths = viewModel.TrialPeriodMonths
            });

        AddFeedback(result, "Membership settings updated");

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/messages/{id:guid}/replied")]
    public async Task<IActionResult> MarkMessageAsReplied(Guid chapterId, Guid id)
    {
        var request = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        var result = await _chapterAdminService.SetMessageAsReplied(request, id);
        AddFeedback(result, "Message updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/messages/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToMessage(Guid chapterId, Guid id, 
        [FromForm] ChapterMessageReplyFormViewModel viewModel)
    {
        var request = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        var result = await _chapterAdminService.ReplyToMessage(request, id, viewModel.Message ?? "");
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpGet("groups/{groupId}/payments/sessions/{id}/status")]
    public async Task<IActionResult> GetPaymentSessionStatus(string id, Guid groupId)
    {
        var request = MemberChapterServiceRequest(groupId);
        var status = await _chapterAdminService.GetChapterPaymentCheckoutSessionStatus(request, id);

        return Ok(new
        {
            status = status.ToString()
        });
    }

    [HttpPost("groups/{id:guid}/privacy")]
    public async Task<IActionResult> UpdatePrivacySettings(Guid id, [FromForm] ChapterPrivacySettingsFormViewModel viewModel)
    {
        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        var result = await _chapterAdminService.UpdateChapterPrivacySettings(request, new UpdateChapterPrivacySettings
        {
            Conversations = viewModel.Conversations,
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
        var request = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        var model = new CreateChapterQuestion
        {
            Answer = answer ?? "",
            Name = name ?? ""
        };
        var result = await _chapterAdminService.CreateChapterQuestion(request, model);
        AddFeedback(result, "Question created");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/delete")]
    public async Task<IActionResult> DeleteQuestion(Guid chapterId, Guid questionId)
    {
        var request = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        await _chapterAdminService.DeleteChapterQuestion(request, questionId);
        AddFeedback("Question deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/move/down")]
    public async Task<IActionResult> MoveQuestionDown(Guid chapterId, Guid questionId)
    {
        var request = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(request,
            questionId, 1);
        AddFeedback("Question order updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/move/up")]
    public async Task<IActionResult> MoveQuestionUp(Guid chapterId, Guid questionId)
    {
        var request = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(request,
            questionId, -1);
        AddFeedback("Question order updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var request = new MemberChapterServiceRequest(id, MemberServiceRequest);
        var result = await _chapterAdminService.PublishChapter(request);
        AddFeedback(result, "Group published");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/messages/{id}/delete")]
    public async Task<IActionResult> DeleteContactRequest(Guid chapterId, Guid id)
    {
        var serviceRequest = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        var result = await _chapterAdminService.DeleteChapterContactMessage(serviceRequest, id);
        AddFeedback(result, "Message deleted");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/theme")]
    public async Task<IActionResult> UpdateTheme(Guid chapterId, ThemeFormViewModel viewModel)
    {
        var serviceRequest = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        var result = await _chapterAdminService.UpdateChapterTheme(serviceRequest, new UpdateChapterTheme
        {
            Background = viewModel.Background,
            Color = viewModel.Color
        });
        AddFeedback(result, "Theme updated");
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

    [HttpPost("groups/{chapterId:guid}/properties/{id:guid}/delete")]
    public async Task<IActionResult> DeleteProperty(Guid chapterId, Guid id)
    {
        var serviceRequest = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        await _chapterAdminService.DeleteChapterProperty(serviceRequest, id);
        AddFeedback("Property deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/properties/{id:guid}/move/down")]
    public async Task<IActionResult> MovePropertyDown(Guid chapterId, Guid id)
    {
        var serviceRequest = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(serviceRequest, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/properties/{id:guid}/move/up")]
    public async Task<IActionResult> MovePropertyUp(Guid chapterId, Guid id)
    {
        var serviceRequest = new MemberChapterServiceRequest(chapterId, MemberServiceRequest);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(serviceRequest, id, -1);
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

    [HttpPost("groups/{id:guid}/texts")]
    public async Task<IActionResult> UpdateChapterTexts(Guid id,
        [FromForm] ChapterTextsFormViewModel viewModel)
    {
        var serviceRequest = new MemberChapterServiceRequest(id, MemberServiceRequest);
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
