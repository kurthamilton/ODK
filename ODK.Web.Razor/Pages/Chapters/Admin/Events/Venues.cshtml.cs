using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenuesModel : AdminPageModel
{
    public VenuesModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
