using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class JoinModel : ChapterPageModel2
{
    private readonly IMemberService _memberService;
    private readonly IRequestCache _requestCache;

    public JoinModel(IMemberService memberService, IRequestCache requestCache)
    {
        _memberService = memberService;
        _requestCache = requestCache;
    }

    public async Task<IActionResult> OnPost(
        string chapterName,
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel,
        [FromForm] PersonalDetailsFormViewModel personalDetailsViewModel,
        [FromForm] MemberImageCropInfo cropInfo,
        [FromForm] IFormFile image)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);

        var imageModel = new UpdateMemberImage
        {
            ImageData = await image.ToByteArrayAsync(),
            MimeType = image.ContentType
        };

        var model = new CreateMemberProfile
        {
            EmailAddress = personalDetailsViewModel.EmailAddress,
            EmailOptIn = personalDetailsViewModel.EmailOptIn,
            FirstName = personalDetailsViewModel.FirstName,
            Image = imageModel,
            ImageCropInfo = cropInfo,
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

        var result = await _memberService.CreateChapterAccount(chapter.Id, model);
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
