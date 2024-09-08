using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class NotificationsModel : ChapterPageModel
{
    public NotificationsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
