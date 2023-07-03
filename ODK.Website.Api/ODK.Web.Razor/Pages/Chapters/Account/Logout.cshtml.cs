using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Web.Common.Account;

namespace ODK.Web.Razor.Pages.Chapters.Account
{
    public class LogoutModel : ChapterPageModel
    {
        private ILoginHandler _loginHandler;

        public LogoutModel(IRequestCache requestCache, ILoginHandler loginHandler)
            : base(requestCache)
        {
            _loginHandler = loginHandler;
        }

        public async Task<IActionResult> OnGet()
        {
            await _loginHandler.Logout(HttpContext);
            return Redirect($"/{Chapter.Name}");
        }
    }
}
