using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Images;
using ODK.Core.Issues;
using ODK.Core.Notifications;
using ODK.Services.Authentication;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Features;
using ODK.Services.Issues;
using ODK.Services.Issues.Models;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Notifications;
using ODK.Services.Subscriptions;
using ODK.Services.Topics.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Account;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Feedback;
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
    private readonly IIssueService _issueService;
    private readonly ILoginHandler _loginHandler;
    private readonly IMemberService _memberService;
    private readonly INotificationService _notificationService;
    private readonly ISiteSubscriptionService _siteSubscriptionService;

    public AccountController(
        IMemberService memberService,
        ILoginHandler loginHandler,
        IAuthenticationService authenticationService,
        IFeatureService featureService,
        ISiteSubscriptionService siteSubscriptionService,
        INotificationService notificationService,
        IIssueService issueService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _authenticationService = authenticationService;
        _featureService = featureService;
        _issueService = issueService;
        _loginHandler = loginHandler;
        _memberService = memberService;
        _notificationService = notificationService;
        _siteSubscriptionService = siteSubscriptionService;
    }

    [AllowAnonymous]
    [HttpPost("account/activate")]
    public async Task<IActionResult> ActivateAccount(
        [FromForm] ActivateFormViewModel viewModel)
    {
        var result = await _authenticationService.ActivateSiteAccountAsync(
            ServiceRequest,
            viewModel.Token,
            viewModel.Password);
        if (!result.Success)
        {
            AddFeedback(result);
            return RedirectToReferrer();
        }

        AddFeedback("Your account has been activated. You can now login.", FeedbackType.Success);
        return Redirect("/account/login");
    }

    [AllowAnonymous]
    [HttpPost("groups/{chapterId:guid}/account/activate")]
    public async Task<IActionResult> ActivateChapterAccount(
        Guid chapterId, [FromForm] ActivateFormViewModel viewModel)
    {
        var result = await _authenticationService.ActivateChapterAccountAsync(
            ChapterServiceRequest,
            viewModel.Token,
            viewModel.Password);
        if (!result.Success)
        {
            AddFeedback(result);
            return RedirectToReferrer();
        }

        AddFeedback("Your account has been activated. You can now login.", FeedbackType.Success);
        return Redirect(OdkRoutes.Account.Login(Chapter));
    }

    [AllowAnonymous]
    [HttpPost("account/create")]
    public async Task<IActionResult> Create(
        [FromForm] PersonalDetailsFormViewModel personalDetails,
        [FromForm] LocationFormViewModel location,
        [FromForm] OAuthDetailsFormViewModel oauth,
        [FromForm] TopicPickerFormSubmitViewModel topics)
    {
        var newTopics = NewTopicModel.Build(topics.NewTopicGroups, topics.NewTopics);

        var model = new AccountCreateModel
        {
            EmailAddress = personalDetails.EmailAddress,
            FirstName = personalDetails.FirstName,
            LastName = personalDetails.LastName,
            Location = location.Lat != null && location.Long != null
                ? new LatLong(location.Lat.Value, location.Long.Value)
                : default(LatLong?),
            LocationName = location.LocationName,
            NewTopics = newTopics,
            OAuthProviderType = oauth.Provider,
            OAuthToken = oauth.Token,
            TopicIds = topics.TopicIds ?? []
        };

        var result = await _memberService.CreateAccount(ServiceRequest, model);

        if (result.Value?.Activated == true)
        {
            AddFeedback(result, "Your account has been created and is now ready to use");
            return Redirect(OdkRoutes.Account.Login(chapter: null));
        }

        return Redirect("/Account/Pending");
    }

    [HttpPost("account/currency")]
    public async Task<IActionResult> UpdateCurrency([FromForm] Guid currencyId)
    {
        var result = await _memberService.UpdateMemberCurrency(CurrentMember.Id, currencyId);
        AddFeedback(result, "Currency updated");
        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("account/login")]
    public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, string? returnUrl)
    {
        var result = await _loginHandler.Login(
            ServiceRequest,
            viewModel.Email ?? string.Empty,
            viewModel.Password ?? string.Empty,
            rememberMe: true);

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
    public async Task<IActionResult> GoogleSiteLogin([FromForm] string token, string? returnUrl)
    {
        var result = await _loginHandler.OAuthLogin(
            ServiceRequest,
            OAuthProviderType.Google,
            token);
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
        var result = await _loginHandler.Login(
            ServiceRequest,
            viewModel.Email ?? string.Empty,
            viewModel.Password ?? string.Empty,
            rememberMe: true);

        if (result.Success && result.Member != null)
        {
            var platform = Platform;

            var redirectUrl = string.IsNullOrEmpty(returnUrl)
                ? OdkRoutes.Groups.Group(Chapter)
                : returnUrl;

            return Redirect(redirectUrl);
        }

        AddFeedback("Username or password incorrect", FeedbackType.Error);

        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("{chapterName}/account/login/google")]
    public async Task<IActionResult> GoogleChapterLogin(string chapterName, [FromForm] string token, string? returnUrl)
    {
        var result = await _loginHandler.OAuthLogin(
            ServiceRequest,
            OAuthProviderType.Google,
            token);
        if (result.Success && result.Member != null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(OdkRoutes.Groups.Group(Chapter));
            }

            return Redirect(returnUrl);
        }

        AddFeedback("Account not registered", FeedbackType.Error);

        return RedirectToReferrer();
    }

    [HttpPost("account/delete")]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await _memberService.DeleteMember(MemberServiceRequest);
        AddFeedback(result, "Account deleted");

        if (!result.Success)
        {
            return RedirectToReferrer();
        }

        await _loginHandler.Logout();
        return Redirect("/");
    }

    [HttpPost("{chapterName}/account/email/change")]
    public async Task<IActionResult> ChangeChapterEmailRequest(
        string chapterName, [FromForm] ChangeEmailFormViewModel viewModel)
    {
        var result = await _memberService.RequestMemberEmailAddressUpdate(
                MemberChapterServiceRequest, viewModel.Email ?? string.Empty);

        var successMessage =
            "An email has been sent to the email address you provided. " +
            "Please complete your update by following the link in the email.";
        AddFeedback(result, successMessage);

        return RedirectToReferrer();
    }

    [HttpPost("account/email/change")]
    public async Task<IActionResult> ChangeEmailRequest([FromForm] ChangeEmailFormViewModel viewModel)
    {
        var result = await _memberService.RequestMemberEmailAddressUpdate(
            MemberServiceRequest, viewModel.Email ?? string.Empty);

        var successMessage =
            "An email has been sent to the email address you provided. " +
            "Please complete your update by following the link in the email.";
        AddFeedback(result, successMessage);

        return RedirectToReferrer();
    }

    [HttpGet("account/email/change/confirm")]
    public async Task<IActionResult> ChangeEmailConfirm(string token)
    {
        var result = await _memberService.ConfirmEmailAddressUpdate(CurrentMember.Id, token);
        AddFeedback(result, "Email address updated");
        return Redirect("/account");
    }

    [HttpGet("{chapterName}/Account/Email/Change/Confirm")]
    public async Task<IActionResult> ChangeEmailConfirm(string chapterName, string token)
    {
        var result = await _memberService.ConfirmEmailAddressUpdate(CurrentMember.Id, token);
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

        var result = await _memberService.UpdateMemberEmailPreferences(CurrentMember.Id, disabledTypes);

        AddFeedback(result, "Email preferences updated");

        return RedirectToReferrer();
    }

    [HttpPost("account/location")]
    public async Task<IActionResult> UpdateLocation([FromForm] LocationFormViewModel viewModel)
    {
        var location = viewModel.Lat != null && viewModel.Long != null
            ? new LatLong(viewModel.Lat.Value, viewModel.Long.Value)
            : default(LatLong?);
        await _memberService.UpdateMemberLocation(
            CurrentMember.Id, location, viewModel.LocationName, viewModel.DistanceUnit);

        AddFeedback("Location updated", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("account/notifications/{group}")]
    public async Task<IActionResult> UpdateNotificationSettings(
        NotificationGroupType group, bool enabled)
    {
        var result = await _notificationService.UpdateMemberNotificationSettings(MemberServiceRequest, group, enabled);
        if (result.Success)
        {
            return NoContent();
        }

        return StatusCode(500);
    }

    [HttpPost("groups/{chapterId:guid}/notifications/{group}")]
    public async Task<IActionResult> UpdateNotificationSettings(
        Guid chapterId, NotificationGroupType group, bool enabled)
    {
        var result = await _notificationService.UpdateMemberChapterNotificationSettings(MemberChapterServiceRequest, group, enabled);
        if (result.Success)
        {
            return NoContent();
        }

        return StatusCode(500);
    }

    [HttpPost("account/notifications/dismiss")]
    public async Task<IActionResult> DismissNotifications()
    {
        await _notificationService.MarkAllAsRead(CurrentMember.Id);
        return Ok();
    }

    [HttpPost("account/notifications/{id:guid}/dismiss")]
    public async Task<IActionResult> DismissNotification(Guid id)
    {
        await _notificationService.MarkAsRead(CurrentMember.Id, id);
        return Ok();
    }

    [HttpPost("account/personaldetails")]
    public async Task<IActionResult> UpdatePersonalDetails([FromForm] PersonalDetailsFormViewModel viewModel)
    {
        var model = new MemberSiteProfileUpdateModel
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName
        };

        var result = await _memberService.UpdateMemberSiteProfile(MemberServiceRequest, model);
        AddFeedback(result, "Personal details updated");

        return result.Success
            ? RedirectToReferrer()
            : View();
    }

    [HttpPost("Account/FeatureTips/{name}/Hide")]
    public async Task<IActionResult> HideFeatureTip(string name)
    {
        await _featureService.MarkAsSeen(MemberServiceRequest, name);
        return Ok();
    }

    [HttpPost("account/issues")]
    public async Task<IActionResult> CreateIssue([FromForm] IssueCreateFormViewModel viewModel)
    {
        var result = await _issueService.CreateIssue(MemberServiceRequest, new IssueCreateModel
        {
            Message = viewModel.Message ?? string.Empty,
            Title = viewModel.Title ?? string.Empty,
            Type = viewModel.Type ?? IssueType.None
        });

        AddFeedback(result, "Issue created");

        return Redirect(OdkRoutes.Account.Issues());
    }

    [HttpPost("account/issues/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToIssue(Guid id, [FromForm] IssueReplyFormViewModel viewModel)
    {
        var result = await _issueService.ReplyToIssue(MemberServiceRequest, id, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("account/password/change")]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordFormViewModel viewModel)
    {
        var result = await _authenticationService.ChangePasswordAsync(CurrentMember.Id,
            viewModel.CurrentPassword ?? string.Empty, viewModel.NewPassword ?? string.Empty);
        AddFeedback(result, "Password changed");
        return RedirectToReferrer();
    }

    [AllowAnonymous]
    [HttpPost("/account/password/forgotten")]
    public async Task<IActionResult> ForgottenPassword([FromForm] ForgottenPasswordFormViewModel viewModel)
    {
        var result = await _authenticationService.RequestPasswordResetAsync(
            ServiceRequest, viewModel.EmailAddress ?? string.Empty);
        string successMessage =
            "An email containing password reset instructions has been sent to that email address " +
            "if it is associated with an account";
        AddFeedback(result, successMessage);

        var fallback = OdkRoutes.Account.Login(chapter: null);
        return RedirectToReferrer(fallback);
    }

    [AllowAnonymous]
    [HttpPost("/{chapterId:guid}/Account/Password/Forgotten")]
    public async Task<IActionResult> ForgottenPassword(Guid chapterId, [FromForm] ForgottenPasswordFormViewModel viewModel)
    {
        var chapter = Chapter;
        var result = await _authenticationService.RequestPasswordResetAsync(
            ServiceRequest,
            chapter,
            viewModel.EmailAddress ?? string.Empty);

        var successMessage =
            "An email containing password reset instructions has been sent to that email address " +
            "if it is associated with an account";
        AddFeedback(result, successMessage);
        return RedirectToReferrer();
    }

    [HttpPost("account/picture/change")]
    public async Task<IActionResult> UpdatePicture([FromForm] string imageDataUrl)
    {
        if (string.IsNullOrEmpty(imageDataUrl))
        {
            AddFeedback("No image provided", FeedbackType.Warning);
            return RedirectToReferrer();
        }

        if (!ImageHelper.TryParseDataUrl(imageDataUrl, out var bytes))
        {
            AddFeedback("Image could not be processed", FeedbackType.Error);
            return RedirectToReferrer();
        }

        var result = await _memberService.UpdateMemberImage(CurrentMember.Id, bytes);
        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("account/picture/rotate")]
    public async Task<IActionResult> RotatePicture()
    {
        await _memberService.RotateMemberImage(CurrentMember.Id);

        return RedirectToReferrer();
    }

    [HttpPost("account/subscription/confirm")]
    public async Task<IActionResult> ConfirmSiteSubscription(
        [FromForm] Guid siteSubscriptionPriceId,
        [FromForm] string externalId)
    {
        var result = await _siteSubscriptionService.ConfirmMemberSiteSubscription(
            MemberServiceRequest,
            siteSubscriptionPriceId,
            externalId);
        AddFeedback(result, "Subscription updated");
        return RedirectToReferrer();
    }

    [HttpPost("chapters/{chapterId:guid}/account/subscription/cancel")]
    public async Task<IActionResult> CancelChapterSubscription(Guid chapterId, [FromForm] CancelSubscriptionRequest form)
    {
        var result = await _memberService.CancelChapterSubscription(CurrentMember.Id, form.ExternalId);
        AddFeedback(result, "Purchase complete. Thank you for subscribing.");
        return RedirectToReferrer();
    }

    [HttpPost("account/topics")]
    public async Task<IActionResult> UpdateTopics([FromForm] TopicPickerFormSubmitViewModel viewModel)
    {
        var newTopics = NewTopicModel.Build(viewModel.NewTopicGroups, viewModel.NewTopics);

        var result = await _memberService.UpdateMemberTopics(
            MemberServiceRequest,
            viewModel.TopicIds ?? [],
            newTopics);
        AddFeedback(result, "Interests updated");
        return RedirectToReferrer();
    }
}