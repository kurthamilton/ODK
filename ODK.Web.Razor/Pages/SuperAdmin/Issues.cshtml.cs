using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class IssuesModel : SuperAdminPageModel
{
    public IssuesModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
