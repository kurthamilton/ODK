using Microsoft.AspNetCore.Mvc;
using ODK.Core.Images;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class JoinModel : OdkPageModel
{
    private readonly IMemberService _memberService;

    public JoinModel(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public string? InviteToken { get; private set; }

    public void OnGet([FromQuery] string? token) => InviteToken = token;

    public async Task<IActionResult> OnPost(
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel,
        [FromForm] PersonalDetailsFormViewModel personalDetailsViewModel)
    {
        // The view loads against this, so a re-render below has to find it here as well as after OnGet.
        InviteToken = personalDetailsViewModel.InviteToken;

        var properties = profileViewModel.Properties.Select(x => x.ToMemberPropertyUpdate());

        /* A signed-in member already has an account and a picture, so this page joins them to the group rather
           than creating anything - the form posts only the group's questions for them. That is the route an
           invitation takes when the member it names already had an account: they sign in and land back here. */
        if (CurrentMemberOrDefault != null)
        {
            var joinResult = await _memberService.JoinChapter(MemberChapterServiceRequest, properties);
            if (!joinResult.Success)
            {
                AddFeedback(joinResult);
                return Page();
            }

            return Redirect(OdkRoutes.Groups.Group(Chapter));
        }

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

        var model = new MemberCreateProfile
        {
            EmailAddress = personalDetailsViewModel.EmailAddress,
            EmailOptIn = personalDetailsViewModel.EmailOptIn,
            FirstName = personalDetailsViewModel.FirstName,
            ImageData = imageData,
            InviteToken = personalDetailsViewModel.InviteToken,
            LastName = personalDetailsViewModel.LastName,
            RecaptchaToken = personalDetailsViewModel.Recaptcha ?? string.Empty,
            Properties = properties
        };

        var result = await _memberService.CreateChapterAccount(ChapterServiceRequest, model);
        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        /* An invitation the member registered the invited address with stands in for the activation email, so
           there is nothing to wait for - they go straight to setting a password. */
        return Redirect(result.ActivationToken != null
            ? OdkRoutes.Account.Activate(Chapter, result.ActivationToken)
            : OdkRoutes.Account.Pending(Chapter));
    }
}