using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class IconsModel : SuperAdminPageModel
{
    public IconsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
