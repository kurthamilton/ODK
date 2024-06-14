using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class EmailsModel : ChapterPageModel
{
    public EmailsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
