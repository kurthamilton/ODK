using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class DeleteModel : ChapterPageModel
{
    public DeleteModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
