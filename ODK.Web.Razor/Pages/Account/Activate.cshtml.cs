using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Pages.Chapters;

namespace ODK.Web.Razor.Pages.Account
{
    public class ActivateModel : ChapterPageModel
    {
        private readonly IAuthenticationService _authenticationService;

        public ActivateModel(IRequestCache requestCache, IAuthenticationService authenticationService) 
            : base(requestCache)
        {
            _authenticationService = authenticationService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string token, ActivateFormViewModel viewModel)
        {
            ServiceResult result = await _authenticationService.ActivateAccountAsync(token, viewModel.Password);
            if (!result.Success)
            {
                AddFeedback(new FeedbackViewModel(result));
                return Page();
            }

            AddFeedback(new FeedbackViewModel("Your account has been activated. You can now login.", FeedbackType.Success));
            return Redirect($"/{Chapter.Name}/Account/Login");
        }
    }
}
