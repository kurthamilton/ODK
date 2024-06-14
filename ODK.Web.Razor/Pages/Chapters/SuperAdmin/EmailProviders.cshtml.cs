using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class EmailProvidersModel : SuperAdminPageModel
{
    public EmailProvidersModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
