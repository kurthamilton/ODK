using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account
{
    public class AccountModel : ChapterPageModel
    {
        public AccountModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
