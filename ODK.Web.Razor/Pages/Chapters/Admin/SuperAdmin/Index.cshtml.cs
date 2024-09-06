using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class IndexModel : AdminPageModel
{
    public IndexModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}