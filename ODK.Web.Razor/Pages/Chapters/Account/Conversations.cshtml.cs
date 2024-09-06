using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class ConversationsModel : ChapterPageModel
{
    public ConversationsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
