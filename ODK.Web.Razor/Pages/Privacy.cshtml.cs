using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages;

public class PrivacyModel : OdkPageModel
{
    public PrivacyModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}