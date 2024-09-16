using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Caching;
using ODK.Services.Features;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Subscriptions;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Account;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Login;
using ODK.Web.Razor.Models.Notifications;
using ODK.Web.Razor.Models.Topics;

namespace ODK.Web.Razor.Controllers;

[Authorize]
[ApiController]
public class AccountController : OdkControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IFeatureService _featureService;
    private readonly ILoginHandler _loginHandler;
    private readonly IMemberService _memberService;
    private readonly INotificationService _notificationService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IRequestCache _requestCache;
    private readonly ISiteSubscriptionService _siteSubscriptionService;

    public AccountController(
        IMemberService memberService, 
        ILoginHandler loginHandler, 
        IRequestCache requestCache,
        IAuthenticationService authenticationService, 
        IFeatureService featureService,
        ISiteSubscriptionService siteSubscriptionService,
        IPlatformProvider platformProvider,
        INotificationService notificationService)
    {
        _authenticationService = authenticationService;
        _featureService = featureService;
        _loginHandler = loginHandler;
        _memberService = memberService;
        _notificationService = notificationService;
        _platformProvider = platformProvider;
        _requestCache = requestCache;
        _siteSubscriptionService = siteSubscriptionService;
    }

    [AllowAnonymous]
    [HttpPost("account/create")]
    public async Task<IActionResult> Create(
        [FromForm] PersonalDetailsFormViewModel personalDetails,
        [FromForm] LocationFormViewModel location,
        [FromForm] OAuthDetailsFormViewModel oauth)
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
            OAuthProviderType = oauth.Provider,
            OAuthToken = oauth.Token,
            TimeZoneId = location.TimeZoneId
        };

        var result = await _memberService.CreateAccount(model);        

        if (result.Value?.Activated == true)
        {
            AddFeedback(result, "Your account has been created and is now ready to use");
            return Redirect(OdkRoutes.Account.Login(null));
        }

        string successMessage =
            "Thank you for signing up. " +
            "An email has been sent to your email address containing a link to activate your account.";
        AddFeedback(result, successMessage);

        return Redirect("/Account/Pending");
    }

    [HttpPost("account/currency")]
    public async Task<IActionResult> UpdateCurrency([FromForm] Guid currencyId)
    {
        var result = await _memberService.UpdateMemberCurrency(MemberId, currencyId);
        AddFeedback(result, "Currency updated");
        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("account/login")]
    public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, string? returnUrl)
    {
        var result = await _loginHandler.Login(viewModel.Email ?? "",
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
    [HttpPost("account/login/google")]
    public async Task<IActionResult> GoogleLogin([FromForm] string token, string? returnUrl)
    {
        var result = await _loginHandler.OAuthLogin(OAuthProviderType.Google, token);
        if (result.Success && result.Member != null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                return Redirect("/");
            }

            return Redirect(returnUrl);
        }

        AddFeedback("Account not registered", FeedbackType.Error);

        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("{chapterName}/Account/Login")]
    public async Task<IActionResult> Login(string chapterName, [FromForm] LoginViewModel viewModel, string? returnUrl)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        var result = await _loginHandler.Login(viewModel.Email ?? "",
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

        await _loginHandler.Logout();
        return Redirect("/");
    }

    [HttpPost("account/email/change")]
    public async Task<IActionResult> ChangeEmailRequest([FromForm] ChangeEmailFormViewModel viewModel)
    {
        Guid? chapterId = null;

        var platform = _platformProvider.GetPlatform();
        if (platform == PlatformType.DrunkenKnitwits)
        {
            var member = await _memberService.GetMember(MemberId);
            chapterId = member.Chapters.Count == 1
                ? member.Chapters.First().ChapterId
                : null;
        }

        var result = chapterId != null
            ? await _memberService.RequestMemberEmailAddressUpdate(MemberId, chapterId.Value, viewModel.Email ?? "")
            : await _memberService.RequestMemberEmailAddressUpdate(MemberId, viewModel.Email ?? "");

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

    [HttpPost("account/emails")]
    public async Task<IActionResult> UpdateEmailPreferences([FromForm] EmailPreferencesFormViewModel viewModel)
    {
        var disabledTypes = viewModel.Preferences
            .Where(x => !x.Enabled)
            .Select(x => x.Type)
            .ToArray();

        var result = await _memberService.UpdateMemberEmailPreferences(MemberId, disabledTypes);

        AddFeedback(result, "Email preferences updated");

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

    [HttpPost("account/notifications")]
    public async Task<IActionResult> UpdateNotificationSettings([FromForm] NotificationSettingsFormViewModel viewModel)
    {
        var disabledTypes = viewModel.Settings
            .Where(x => !x.Enabled)
            .Select(x => x.Type)
            .ToArray();

        var result = await _notificationService.UpdateMemberNotificationSettings(MemberId, disabledTypes);

        AddFeedback(result, "Notification preferences updated");

        return RedirectToReferrer();
    }

    [HttpPost("account/notifications/{id:guid}/dismiss")]
    public async Task<IActionResult> DismissNotification(Guid id)
    {        
        await _notificationService.MarkAsRead(MemberId, id);
        return Ok();
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
        await _memberService.RotateMemberImage(MemberId);

        return RedirectToReferrer();
    }

    [HttpPost("account/subscription/confirm")]
    public async Task<IActionResult> ConfirmSiteSubscription(
        [FromForm] Guid siteSubscriptionPriceId,
        [FromForm] string externalId)
    {
        var result = await _siteSubscriptionService.ConfirmMemberSiteSubscription(
            MemberId, 
            siteSubscriptionPriceId, 
            externalId);
        AddFeedback(result, "Subscription updated");
        return RedirectToReferrer();
    }

    [HttpPost("Chapters/{id:guid}/Account/Subscription/Purchase")]
    public async Task<IActionResult> PurchaseChapterSubscription(Guid id, [FromForm] PurchaseSubscriptionRequest request)
    {
        var result = await _memberService.PurchaseChapterSubscription(MemberId, id, request.SubscriptionId, request.Token);
        AddFeedback(result, "Purchase complete. Thank you for subscribing.");
        return RedirectToReferrer();
    }

    [HttpPost("account/topics")]
    public async Task<IActionResult> UpdateTopics([FromForm] TopicPickerFormSubmitViewModel viewModel)
    {
        var result = await _memberService.UpdateMemberTopics(MemberId, viewModel.TopicIds);
        AddFeedback(result, "Interests updated");
        return RedirectToReferrer();
    }
}
