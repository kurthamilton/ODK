using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class PrivacyModel : AdminPageModel
{
    public PrivacyModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}