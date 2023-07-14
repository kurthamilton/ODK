using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Pages.Chapters.Account
{
    public class PasswordResetModel : ChapterPageModel
    {
        private readonly IAuthenticationService _authenticationService;

        public PasswordResetModel(IRequestCache requestCache, IAuthenticationService authenticationService) 
            : base(requestCache)
        {
            _authenticationService = authenticationService;
        }

        public string? Token { get; private set; }

        public void OnGet(string token)
        {
            Token = token;
        }

        public async Task<IActionResult> OnPostAsync(ResetPasswordFormViewModel viewModel)
        {
            ServiceResult result = await _authenticationService.ResetPassword(viewModel.Token, viewModel.NewPassword);
            if (result.Success)
            {
                AddFeedback(new FeedbackViewModel("Your password has been updated.", FeedbackType.Success));
                return Redirect($"/{Chapter.Name}/Account/Login");
            }

            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }
    }
}
