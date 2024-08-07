using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class FeaturesModel : SuperAdminPageModel
{
    public FeaturesModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }
}
