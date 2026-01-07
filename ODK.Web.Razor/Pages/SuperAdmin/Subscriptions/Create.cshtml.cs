using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin.Subscriptions;

public class CreateModel : SuperAdminPageModel
{
    public CreateModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
