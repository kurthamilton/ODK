using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class EmailsModel : SuperAdminPageModel
{
    public EmailsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
