using Microsoft.AspNetCore.Mvc;
using ODK.Core.Images;
using ODK.Services;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class JoinModel : ChapterPageModel2
{
    private readonly IMemberService _memberService;

    public JoinModel(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public async Task<IActionResult> OnPost(
        string chapterName,
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel,
        [FromForm] PersonalDetailsFormViewModel personalDetailsViewModel)
    {
        if (string.IsNullOrEmpty(profileViewModel.ImageDataUrl))
        {
            AddFeedback("No image provided", FeedbackType.Warning);
            return Page();
        }

        if (!ImageHelper.TryParseDataUrl(profileViewModel.ImageDataUrl, out var imageData))
        {
            AddFeedback("Image could not be processed", FeedbackType.Error);
            return Page();
        }

        var model = new CreateMemberProfile
        {
            EmailAddress = personalDetailsViewModel.EmailAddress,
            EmailOptIn = personalDetailsViewModel.EmailOptIn,
            FirstName = personalDetailsViewModel.FirstName,
            ImageData = imageData,
            LastName = personalDetailsViewModel.LastName,
            Properties = profileViewModel.Properties.Select(x => new UpdateMemberProperty
            {
                ChapterPropertyId = x.ChapterPropertyId,
                Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
                    ? x.OtherValue ?? ""
                    : x.Value ?? ""
            })
        };

        var chapter = await GetChapter();
        var result = await _memberService.CreateChapterAccount(ServiceRequest, chapter.Id, model);
        PostJoin(result);
        return result.Success
            ? Redirect($"/{chapterName}/Account/Pending")
            : Page();
    }

    private void PostJoin(ServiceResult result)
    {
        string successMessage =
            "Thank you for signing up. " +
            "An email has been sent to your email address containing a link to activate your account.";
        AddFeedback(result, successMessage);
    }
}