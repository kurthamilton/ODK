using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

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
