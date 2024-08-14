using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Services.Features;
using ODK.Services.Members;
using ODK.Services.Users.ViewModels;
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
    [HttpPost("account/create")]
    public async Task<IActionResult> Create(
        [FromForm] PersonalDetailsFormViewModel personalDetails,
        [FromForm] LocationFormViewModel location)
    {
        var model = new CreateAccountModel
        {
            EmailAddress = personalDetails.EmailAddress,            
            FirstName = personalDetails.FirstName,            
            LastName = personalDetails.LastName,
            Location = location.Lat != null && location.Long != null 
                ? new LatLong(location.Lat.Value, location.Long.Value)
                : default(LatLong?),
            LocationName = location.Name,
            TimeZoneId = location.TimeZoneId
        };

        var result = await _memberService.CreateAccount(model);
        PostJoin(result);
        return Redirect($"/Account/Pending");
    }

    [AllowAnonymous]
    [HttpPost("{chapterName}/Account/Join")]
    public async Task<IActionResult> Join(
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

        var result = await _memberService.CreateMember(chapter.Id, model);
        PostJoin(result);
        return Redirect($"/{chapterName}/Account/Pending");
    }

    [AllowAnonymous]
    [HttpPost("account/login")]
    public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, string? returnUrl)
    {
        var result = await _loginHandler.Login(HttpContext, viewModel.Email ?? "",
            viewModel.Password ?? "", true);

        if (result.Success && result.Member != null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                return Redirect("/");
            }

            return Redirect(returnUrl);
        }

        AddFeedback("Username or password incorrect", FeedbackType.Error);

        return RedirectToReferrer();
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

        AddFeedback("Username or password incorrect", FeedbackType.Error);

        return RedirectToReferrer();
    }

    [HttpPost("account/delete")]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await _memberService.DeleteMember(MemberId);
        AddFeedback(result, "Account deleted");

        if (!result.Success)
        {            
            return RedirectToReferrer();
        }

        await _loginHandler.Logout(HttpContext);
        return Redirect("/");
    }

    [HttpPost("account/email/change")]
    public async Task<IActionResult> ChangeEmailRequest([FromForm] ChangeEmailFormViewModel viewModel)
    {
        var result = await _memberService.RequestMemberEmailAddressUpdate(MemberId, viewModel.Email ?? "");
        var successMessage =
            "An email has been sent to the email address you provided. " +
            "Please complete your update by following the link in the email.";
        AddFeedback(result, successMessage);

        return RedirectToReferrer();
    }

    [HttpPost("{ChapterName}/Account/Email/Change")]
    public async Task<IActionResult> ChangeEmailRequest(string chapterName, [FromForm] ChangeEmailFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);

        var result = await _memberService.RequestMemberEmailAddressUpdate(MemberId, chapter.Id, viewModel.Email ?? "");
        var successMessage =
            "An email has been sent to the email address you provided. " +
            "Please complete your update by following the link in the email.";
        AddFeedback(result, successMessage);
        return RedirectToReferrer();
    }

    [HttpGet("account/email/change/confirm")]
    public async Task<IActionResult> ChangeEmailConfirm(string token)
    {
        var result = await _memberService.ConfirmEmailAddressUpdate(MemberId, token);
        AddFeedback(result, "Email address updated");
        return Redirect("/account");
    }

    [HttpGet("{chapterName}/Account/Email/Change/Confirm")]
    public async Task<IActionResult> ChangeEmailConfirm(string chapterName, string token)
    {
        var result = await _memberService.ConfirmEmailAddressUpdate(MemberId, token);
        AddFeedback(result, "Email address updated");
        return Redirect($"/{chapterName}/Account");
    }

    [HttpPost("{ChapterName}/Account/Emails/Subscribe")]
    public async Task<IActionResult> SubscribeToEmails()
    {
        await _memberService.UpdateMemberEmailOptIn(MemberId, true);
        return RedirectToReferrer();
    }

    [HttpPost("{ChapterName}/Account/Emails/Unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromEmails()
    {
        await _memberService.UpdateMemberEmailOptIn(MemberId, false);
        return RedirectToReferrer();
    }

    [HttpPost("account/location")]
    public async Task<IActionResult> UpdateLocation([FromForm] LocationFormViewModel viewModel)
    {
        var location = viewModel.Lat != null && viewModel.Long != null
            ? new LatLong(viewModel.Lat.Value, viewModel.Long.Value) 
            : default(LatLong?);
        await _memberService.UpdateMemberLocation(MemberId, location, viewModel.Name, viewModel.DistanceUnit);

        AddFeedback("Location updated", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("account/personaldetails")]
    public async Task<IActionResult> UpdatePersonalDetails([FromForm] PersonalDetailsFormViewModel viewModel)
    {
        var model = new UpdateMemberSiteProfile
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName
        };

        var memberId = User.MemberId();

        var result = await _memberService.UpdateMemberSiteProfile(memberId, model);
        AddFeedback(result, "Profile updated");

        return result.Success
            ? RedirectToReferrer()
            : View();        
    }

    [HttpPost("{ChapterName}/Account/Profile")]
    public async Task<IActionResult> UpdateChapterProfile(
        string chapterName, 
        [FromForm] ChapterProfileFormSubmitViewModel profileViewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var model = new UpdateMemberChapterProfile
        {
            Properties = profileViewModel.Properties.Select(x => new UpdateMemberProperty
            {
                ChapterPropertyId = x.ChapterPropertyId,
                Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
                    ? x.OtherValue ?? ""
                    : x.Value ?? ""
            })
        };

        var memberId = User.MemberId();

        var result = await _memberService.UpdateMemberChapterProfile(memberId, chapter.Id, model);
        AddFeedback(result, "Profile updated");        
        return result.Success ? RedirectToReferrer() : View();        
    }

    [HttpPost("Account/FeatureTips/{name}/Hide")]
    public async Task<IActionResult> HideFeatureTip(string name)
    {
        await _featureService.MarkAsSeen(MemberId, name);
        return Ok();
    }    

    [HttpPost("account/password/change")]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordFormViewModel viewModel)
    {
        var result = await _authenticationService.ChangePasswordAsync(MemberId, 
            viewModel.CurrentPassword ?? "", viewModel.NewPassword ?? "");
        AddFeedback(result, "Password changed");
        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("/account/password/forgotten")]
    public async Task<IActionResult> ForgottenPassword([FromForm] ForgottenPasswordFormViewModel viewModel)
    {
        var result = await _authenticationService.RequestPasswordResetAsync(viewModel.EmailAddress ?? "");
        string successMessage = 
            "An email containing password reset instructions has been sent to that email address " +
            "if it is associated with an account";
        AddFeedback(result, successMessage);
        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("/{ChapterName}/Account/Password/Forgotten")]
    public async Task<IActionResult> ForgottenPassword(string chapterName, [FromForm] ForgottenPasswordFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _authenticationService.RequestPasswordResetAsync(chapter.Id, viewModel.EmailAddress ?? "");
        var successMessage = 
            "An email containing password reset instructions has been sent to that email address " +
            "if it is associated with an account";
        AddFeedback(result, successMessage);
        return RedirectToReferrer();
    }

    [HttpPost("account/picture/change")]
    public async Task<IActionResult> UpdatePicture([FromForm] MemberImageCropInfo cropInfo, [FromForm] IFormFile? image)
    {
        return await UpdatePicture("", cropInfo, image);
    }

    [HttpPost("{chapterName}/Account/Picture/Change")]
    public async Task<IActionResult> UpdatePicture(string chapterName, 
        [FromForm] MemberImageCropInfo cropInfo, [FromForm] IFormFile? image)
    {
        var model = image != null ? new UpdateMemberImage
        {            
            ImageData = await image.ToByteArrayAsync(),
            MimeType = image.ContentType
        } : null;

        var result = await _memberService.UpdateMemberImage(MemberId, model, cropInfo);
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("account/picture/rotate")]
    public async Task<IActionResult> RotatePicture()
    {
        return await RotatePicture("");
    }

    [HttpPost("{chapterName}/Account/Picture/Rotate")]
    public async Task<IActionResult> RotatePicture(string chapterName)
    {
        await _memberService.RotateMemberImage(MemberId);

        return RedirectToReferrer();
    }
    
    [HttpPost("{ChapterName}/Account/Subscription/Purchase")]
    public async Task<IActionResult> PurchaseSubscription(string chapterName, [FromForm] PurchaseSubscriptionRequest request)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _memberService.PurchaseSubscription(MemberId, chapter.Id, request.SubscriptionId, request.Token);
        AddFeedback(result, "Purchase complete. Thank you for subscribing.");
        return RedirectToReferrer();
    }

    private void PostJoin(ServiceResult result)
    {
        string successMessage = 
            "Thank you for signing up. " +
            "An email has been sent to your email address containing a link to activate your account.";
        AddFeedback(result, successMessage);
    }
}
