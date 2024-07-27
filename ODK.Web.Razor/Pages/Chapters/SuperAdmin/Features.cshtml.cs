using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class FeaturesModel : SuperAdminPageModel
{    
    public FeaturesModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }    
}
