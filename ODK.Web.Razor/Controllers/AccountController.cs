using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Web.Common.Account;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Login;

namespace ODK.Web.Razor.Controllers
{
    [Authorize]
    [ApiController]
    public class AccountController : OdkControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILoginHandler _loginHandler;
        private readonly IMemberService _memberService;
        private readonly IRequestCache _requestCache;

        public AccountController(IMemberService memberService, ILoginHandler loginHandler, IRequestCache requestCache,
            IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            _loginHandler = loginHandler;
            _memberService = memberService;
            _requestCache = requestCache;
        }

        [AllowAnonymous]
        [HttpPost("/Account/Login")]
        public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, string? returnUrl)
        {
            AuthenticationResult result = await _loginHandler.Login(HttpContext, viewModel.Email,
                viewModel.Password, true);

            if (result.Success)
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    Chapter chapter = await _requestCache.GetChapter(result.Member.ChapterId);
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
        public async Task<IActionResult> RequestEmailChange([FromForm] ChangeEmailFormViewModel viewModel)
        {
            ServiceResult result = await _memberService.RequestMemberEmailAddressUpdate(MemberId, viewModel.Email);
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

        [AllowAnonymous]
        [HttpPost("{chapterName}/Account/Join")]
        public async Task<IActionResult> Join(string chapterName, [FromForm] ProfileFormViewModel viewModel, [FromForm] IFormFile image)
        {
            Chapter? chapter = await _requestCache.GetChapter(chapterName);
            if (chapter == null)
            {
                AddFeedback(new FeedbackViewModel("An error has occurred", FeedbackType.Error));
                return RedirectToReferrer();
            }

            ServiceResult result = await _memberService.CreateMember(chapter.Id, new CreateMemberProfile
            {
                EmailAddress = viewModel.EmailAddress,
                EmailOptIn = viewModel.EmailOptIn,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Image = new UpdateMemberImage
                {
                    ImageData = await image.ToByteArrayAsync(),
                    MimeType = image.ContentType
                },
                Properties = viewModel.Properties.Select(x => new UpdateMemberProperty
                {
                    ChapterPropertyId = x.ChapterPropertyId,
                    Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(x.OtherValue) ? x.OtherValue : x.Value
                })
            });

            if (result.Success)
            {
                AddFeedback(new FeedbackViewModel("Thank you for signing up. An email has been sent to your email address containing a link to activate your account.", FeedbackType.Success));
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
            ServiceResult result = await _authenticationService.ChangePassword(MemberId, viewModel.CurrentPassword, viewModel.NewPassword);
            AddFeedback(result.Success
                ? new FeedbackViewModel("Password changed", FeedbackType.Success)
                : new FeedbackViewModel(result));

            return RedirectToReferrer();
        }

        [AllowAnonymous]
        [HttpPost("/Account/Password/Forgotten")]
        public async Task<IActionResult> ForgottenPassword([FromForm] ForgottenPasswordFormViewModel viewModel)
        {
            ServiceResult result = await _authenticationService.RequestPasswordReset(viewModel.EmailAddress);
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
        public async Task<IActionResult> UpdatePicture(List<IFormFile> files)
        {
            if (files.Count == 0)
            {
                return RedirectToReferrer();
            }

            IFormFile file = files[0];
            
            UpdateMemberImage update = new UpdateMemberImage
            {
                ImageData = await file.ToByteArrayAsync(),
                MimeType = file.ContentType
            };

            await _memberService.UpdateMemberImage(MemberId, update);

            return RedirectToReferrer();
        }

        [HttpPost("{ChapterName}/Account/Picture/Rotate")]
        public async Task<IActionResult> RotatePicture()
        {
            await _memberService.RotateMemberImage(MemberId, 90);

            return RedirectToReferrer();
        }
        
        [HttpPost("{ChapterName}/Account/Subscription/Purchase")]
        public async Task<IActionResult> PurchaseSubscription([FromForm] PurchaseSubscriptionRequest request)
        {
            ServiceResult result = await _memberService.PurchaseSubscription(MemberId, request.SubscriptionId, request.Token);
            AddFeedback(result.Success
                ? new FeedbackViewModel("Purchase complete. Thank you for subscribing.", FeedbackType.Success)
                : new FeedbackViewModel(result));

            return RedirectToReferrer();
        }
    }
}
