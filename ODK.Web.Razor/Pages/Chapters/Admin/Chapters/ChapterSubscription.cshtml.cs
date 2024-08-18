using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionModel : AdminPageModel
{
    public ChapterSubscriptionModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
