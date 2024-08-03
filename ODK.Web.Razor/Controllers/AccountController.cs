using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Images;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Services.Features;
using ODK.Services.Members;
using ODK.Web.Common.Account;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Login;

namespace ODK.Web.Razor.Controllers;

[Authorize]
[ApiController]
public class AccountController : OdkControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IFeatureService _featureService;
    private readonly ILoginHandler _loginHandler;
    private readonly IMemberService _memberService;
    private readonly IRequestCache _requestCache;

    public AccountController(IMemberService memberService, ILoginHandler loginHandler, IRequestCache requestCache,
        IAuthenticationService authenticationService, IFeatureService featureService)
    {
        _authenticationService = authenticationService;
        _featureService = featureService;
        _loginHandler = loginHandler;
        _memberService = memberService;
        _requestCache = requestCache;
    }

    [AllowAnonymous]
    [HttpPost("{chapterName}/Account/Login")]
    public async Task<IActionResult> Login(string chapterName, [FromForm] LoginViewModel viewModel, string? returnUrl)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _loginHandler.Login(HttpContext, viewModel.Email ?? "",
            viewModel.Password ?? "", true);

        if (result.Success && result.Member != null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                return Redirect($"/{chapter.Name}");
            }

            return Redirect(returnUrl);
        }

        AddFeedback(new FeedbackViewModel("Username or password incorrect", FeedbackType.Error));

        return RedirectToReferrer();
    }

    [HttpPost("{chapterName}/Account/Delete")]
    public async Task<IActionResult> DeleteAccount(string chapterName)
    {
        await _memberService.DeleteMember(MemberId);
        await _loginHandler.Logout(HttpContext);
        return Redirect($"/{chapterName}");
    }

    [HttpPost("{ChapterName}/Account/Email/Change")]
    public async Task<IActionResult> RequestEmailChange(string chapterName, [FromForm] ChangeEmailFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);

        var result = await _memberService.RequestMemberEmailAddressUpdate(MemberId, chapter.Id, viewModel.Email ?? "");
        if (result.Success)
        {
            string message = !string.IsNullOrEmpty(result.Message)
                ? result.Message
                : "An email has been sent to the email address you provided. " +
                  "Please complete your update by following the link in the email.";
            AddFeedback(new FeedbackViewModel(message, FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpGet("{chapterName}/Account/Email/Change/Confirm")]
    public async Task<IActionResult> ConfirmEmailChange(string chapterName, string token)
    {
        ServiceResult result = await _memberService.ConfirmEmailAddressUpdate(MemberId, token);
        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Email address updated", FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return Redirect($"/{chapterName}/Account");
    }

    [HttpPost("/{ChapterName}/Account/Emails/Subscribe")]
    public async Task<IActionResult> SubscribeToEmails()
    {
        await _memberService.UpdateMemberEmailOptIn(MemberId, true);
        return RedirectToReferrer();
    }

    [HttpPost("/{ChapterName}/Account/Emails/Unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromEmails()
    {
        await _memberService.UpdateMemberEmailOptIn(MemberId, false);
        return RedirectToReferrer();
    }

    [HttpPost("Account/FeatureTips/{name}/Hide")]
    public async Task<IActionResult> HideFeatureTip(string name)
    {
        await _featureService.MarkAsSeen(MemberId, name);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("{chapterName}/Account/Join")]
    public async Task<IActionResult> Join(
        string chapterName, 
        [FromForm] ProfileFormViewModel viewModel, 
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
            EmailAddress = viewModel.EmailAddress,
            EmailOptIn = viewModel.EmailOptIn,
            FirstName = viewModel.FirstName,
            Image = imageModel,
            ImageCropInfo = cropInfo,
            LastName = viewModel.LastName,
            Properties = viewModel.Properties.Select(x => new UpdateMemberProperty
            {
                ChapterPropertyId = x.ChapterPropertyId,
                Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
                    ? x.OtherValue ?? ""
                    : x.Value ?? ""
            })
        };

        var result = await _memberService.CreateMember(chapter.Id, model);

        if (result.Success)
        {
            string message = "Thank you for signing up. " +
                             "An email has been sent to your email address containing a link to activate your account.";
            AddFeedback(new FeedbackViewModel(message, FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return Redirect($"/{chapterName}/Account/Pending");
    }

    [HttpPost("{ChapterName}/Account/Password/Change")]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordFormViewModel viewModel)
    {
        ServiceResult result = await _authenticationService.ChangePasswordAsync(MemberId, 
            viewModel.CurrentPassword ?? "", viewModel.NewPassword ?? "");
        AddFeedback(result.Success
            ? new FeedbackViewModel("Password changed", FeedbackType.Success)
            : new FeedbackViewModel(result));

        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("/{ChapterName}/Account/Password/Forgotten")]
    public async Task<IActionResult> ForgottenPassword(string chapterName, [FromForm] ForgottenPasswordFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _authenticationService.RequestPasswordResetAsync(chapter.Id, viewModel.EmailAddress ?? "");
        if (result.Success)
        {
            string message = "An email containing password reset instructions has been sent to that email address " +
                             "if it is associated with an account";
            AddFeedback(new FeedbackViewModel(message, FeedbackType.Success));
        }
        else
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToReferrer();
    }

    [HttpPost("{ChapterName}/Account/Picture/Change")]
    public async Task<IActionResult> UpdatePicture([FromForm] MemberImageCropInfo cropInfo, [FromForm] IFormFile? image)
    {
        var model = image != null ? new UpdateMemberImage
        {            
            ImageData = await image.ToByteArrayAsync(),
            MimeType = image.ContentType
        } : null;

        var result = await _memberService.UpdateMemberImage(MemberId, model, cropInfo);
        AddFeedback(new FeedbackViewModel(result));
        return RedirectToReferrer();
    }

    [HttpPost("{ChapterName}/Account/Picture/Rotate")]
    public async Task<IActionResult> RotatePicture()
    {
        await _memberService.RotateMemberImage(MemberId);

        return RedirectToReferrer();
    }
    
    [HttpPost("{ChapterName}/Account/Subscription/Purchase")]
    public async Task<IActionResult> PurchaseSubscription(string chapterName, [FromForm] PurchaseSubscriptionRequest request)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);

        var result = await _memberService.PurchaseSubscription(MemberId, chapter.Id, request.SubscriptionId, request.Token);
        AddFeedback(result.Success
            ? new FeedbackViewModel("Purchase complete. Thank you for subscribing.", FeedbackType.Success)
            : new FeedbackViewModel(result));

        return RedirectToReferrer();
    }
}
