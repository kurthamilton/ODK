using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account
{
    public class LoginModel : ChapterPageModel
    {
        public LoginModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public string? ReturnUrl { get; private set; }

        public void OnGet(string? returnUrl)
        {
            ReturnUrl = returnUrl;
        }
    }
}
