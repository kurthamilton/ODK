using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Web.Common.Account;
using ODK.Web.Common.Extensions;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Feedback;
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

        [HttpPost("{ChapterName}/Account/Password/Change")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordFormViewModel viewModel)
        {
            ServiceResult result = await _authenticationService.ChangePassword(MemberId, viewModel.CurrentPassword, viewModel.NewPassword);
            AddFeedback(result.Success
                ? new FeedbackViewModel("Password changed", FeedbackType.Success)
                : new FeedbackViewModel(result));

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

        [HttpPost("{ChapterName}/Account/Profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileFormViewModel viewModel)
        {
            UpdateMemberProfile update = new UpdateMemberProfile
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Properties = viewModel.Properties.Select(x => new UpdateMemberProperty
                {
                    ChapterPropertyId = x.ChapterPropertyId,
                    Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(x.OtherValue) ? x.OtherValue : x.Value
                })
            };

            await _memberService.UpdateMemberProfile(MemberId, update);

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
