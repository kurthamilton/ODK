using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class JoinModel : ChapterPageModel
{
    public JoinModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
