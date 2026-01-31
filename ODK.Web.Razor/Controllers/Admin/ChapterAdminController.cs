using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Images;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Emails;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Admin.Members;
using ODK.Web.Razor.Models.Chapters.SiteAdmin;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.Admin;

public class ChapterAdminController : AdminControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IEmailAdminService _emailAdminService;

    public ChapterAdminController(
        IChapterAdminService chapterAdminService,
        IEmailAdminService emailAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterAdminService = chapterAdminService;
        _emailAdminService = emailAdminService;
    }

    [HttpPost("groups/{chapterId:guid}/delete")]
    public async Task<IActionResult> Delete(Guid chapterId)
    {
        var request = MemberServiceRequest;
        var result = await _chapterAdminService.DeleteChapter(request, chapterId);
        AddFeedback(result, "Group deleted");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        return Redirect(OdkRoutes.GroupAdmin.Index());
    }

    [HttpPost("groups/{chapterId:guid}/image")]
    public async Task<IActionResult> UpdateImage(Guid chapterId, [FromForm] ChapterImageFormViewModel viewModel)
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

        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MemberImage, MemberChapterServiceRequest);
        var result = await _chapterAdminService.UpdateChapterImage(request, new UpdateChapterImage
        {
            ImageData = bytes
        });

        AddFeedback(result, "Image updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/links")]
    public async Task<IActionResult> UpdatePrivacySettings(Guid chapterId, [FromForm] ChapterLinksFormSubmitViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.PrivacySettings, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterLinks(request, new UpdateChapterLinks
        {
            Facebook = viewModel.Facebook ?? string.Empty,
            Instagram = viewModel.Instagram ?? string.Empty,
            InstagramFeed = viewModel.ShowInstagramFeed,
            Twitter = viewModel.Twitter ?? string.Empty,
            WhatsApp = viewModel.WhatsApp ?? string.Empty
        });

        AddFeedback("Social media links updated", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/location")]
    public async Task<IActionResult> UpdateLocation(Guid chapterId, [FromForm] ChapterLocationFormViewModel viewModel)
    {
        var lat = viewModel.Lat;
        var lng = viewModel.Long;

        var location = lat != null && lng != null
            ? new LatLong(lat.Value, lng.Value)
            : default(LatLong?);

        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Location, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterLocation(request, location, viewModel.LocationName);

        AddFeedback("Location updated", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/membership")]
    public async Task<IActionResult> UpdateMembershipSettings(Guid chapterId,
        [FromForm] MembershipSettingsFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.MembershipSettings, MemberChapterServiceRequest);
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
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.ContactMessages, MemberChapterServiceRequest);
        var result = await _chapterAdminService.SetMessageAsReplied(request, id);
        AddFeedback(result, "Message updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/messages/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToMessage(Guid chapterId, Guid id,
        [FromForm] ChapterMessageReplyFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.ContactMessages, MemberChapterServiceRequest);
        var result = await _chapterAdminService.ReplyToMessage(request, id, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/pages")]
    public async Task<IActionResult> UpdatePages(Guid chapterId, [FromForm] ChapterPagesFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Pages, MemberChapterServiceRequest);
        var result = await _chapterAdminService.UpdateChapterPages(request, new UpdateChapterPages
        {
            Pages = viewModel.Pages.Select(x => new UpdateChapterPage
            {
                Hidden = x.Hidden,
                Title = x.Title,
                Type = x.Type
            }).ToArray()
        });
        AddFeedback(result, "Pages updated");
        return RedirectToReferrer();
    }

    [HttpGet("groups/{chapterId:guid}/payments/sessions/{id}/status")]
    public async Task<IActionResult> GetPaymentSessionStatus(string id, Guid chapterId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.SiteSubscription, MemberChapterServiceRequest);
        var status = await _chapterAdminService.GetChapterPaymentCheckoutSessionStatus(request, id);

        return Ok(new
        {
            status = status.ToString()
        });
    }

    [HttpPost("groups/{chapterId:guid}/privacy")]
    public async Task<IActionResult> UpdatePrivacySettings(
        Guid chapterId, [FromForm] ChapterPrivacySettingsFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.PrivacySettings, MemberChapterServiceRequest);
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
        var model = new CreateChapterQuestion
        {
            Answer = answer ?? string.Empty,
            Name = name ?? string.Empty
        };

        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Questions, MemberChapterServiceRequest);
        var result = await _chapterAdminService.CreateChapterQuestion(request, model);
        AddFeedback(result, "Question created");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/delete")]
    public async Task<IActionResult> DeleteQuestion(Guid chapterId, Guid questionId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Questions, MemberChapterServiceRequest);
        await _chapterAdminService.DeleteChapterQuestion(request, questionId);
        AddFeedback("Question deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/move/down")]
    public async Task<IActionResult> MoveQuestionDown(Guid chapterId, Guid questionId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Questions, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(request,
            questionId, 1);
        AddFeedback("Question order updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/questions/{questionId:guid}/move/up")]
    public async Task<IActionResult> MoveQuestionUp(Guid chapterId, Guid questionId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Questions, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterQuestionDisplayOrder(request,
            questionId, -1);
        AddFeedback("Question order updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid chapterId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Publish, MemberChapterServiceRequest);
        var result = await _chapterAdminService.PublishChapter(request);
        AddFeedback(result, "Group published");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/messages/{id}/delete")]
    public async Task<IActionResult> DeleteContactRequest(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.ContactMessages, MemberChapterServiceRequest);
        var result = await _chapterAdminService.DeleteChapterContactMessage(request, id);
        AddFeedback(result, "Message deleted");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/theme")]
    public async Task<IActionResult> UpdateTheme(Guid chapterId, ThemeFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Branding, MemberChapterServiceRequest);
        var result = await _chapterAdminService.UpdateChapterTheme(request, new UpdateChapterTheme
        {
            Background = viewModel.Background,
            Color = viewModel.Color
        });
        AddFeedback(result, "Theme updated");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/emails/{type}/restoreDefault")]
    public async Task<IActionResult> RestoreDefaultEmail(Guid chapterId, EmailType type)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Emails, MemberChapterServiceRequest);
        var result = await _emailAdminService.DeleteChapterEmail(request, type);
        AddFeedback(result, "Default email restored");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/emails/{type}/test")]
    public async Task<IActionResult> SendTestEmail(Guid chapterId, EmailType type)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Emails, MemberChapterServiceRequest);
        var result = await _emailAdminService.SendTestEmail(request, type);
        AddFeedback(result, "Test email sent");
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/owner")]
    public async Task<IActionResult> SetOwner(Guid chapterId, [FromForm] Guid memberId)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Emails, MemberChapterServiceRequest);
        await _chapterAdminService.SetOwner(request, memberId);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/properties/{id:guid}/delete")]
    public async Task<IActionResult> DeleteProperty(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Properties, MemberChapterServiceRequest);
        await _chapterAdminService.DeleteChapterProperty(request, id);
        AddFeedback("Property deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/properties/{id:guid}/move/down")]
    public async Task<IActionResult> MovePropertyDown(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Properties, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(request, id, 1);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/properties/{id:guid}/move/up")]
    public async Task<IActionResult> MovePropertyUp(Guid chapterId, Guid id)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Properties, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterPropertyDisplayOrder(request, id, -1);
        return RedirectToReferrer();
    }

    [HttpPost("groups/{chapterId:guid}/texts")]
    public async Task<IActionResult> UpdateChapterTexts(Guid chapterId,
        [FromForm] ChapterTextsFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Texts, MemberChapterServiceRequest);
        var result = await _chapterAdminService.UpdateChapterTexts(request, new UpdateChapterTexts
        {
            Description = viewModel.Description,
            RegisterText = viewModel.RegisterMessage,
            WelcomeText = viewModel.WelcomeMessage
        });

        AddFeedback(result, "Texts updated");

        return RedirectToReferrer();
    }
}