using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class SettingsModel : AdminPageModel
{
    public SettingsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
