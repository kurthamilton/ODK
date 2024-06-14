using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class PendingModel : ChapterPageModel
{
    public PendingModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
