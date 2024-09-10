using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class TopicsModel : SuperAdminPageModel
{
    public TopicsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
