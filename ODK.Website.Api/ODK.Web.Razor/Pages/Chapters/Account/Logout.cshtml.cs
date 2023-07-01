using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters.Account
{
    public class LogoutModel : ChapterPageModel
    {
        public LogoutModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public async Task<IActionResult> OnGet()
        {
            await HttpContext.SignOutAsync();

            return Redirect($"/{Chapter!.Name}");
        }
    }
}
