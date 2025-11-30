using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class PaymentsModel : SuperAdminPageModel
{
    public PaymentsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
