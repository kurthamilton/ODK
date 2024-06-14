using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class ChangePasswordModel : ChapterPageModel
{
    public ChangePasswordModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
