using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class ChangeEmailModel : ChapterPageModel
{
    public ChangeEmailModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
