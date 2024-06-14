using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class ForgottenPasswordModel : ChapterPageModel
{
    public ForgottenPasswordModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
