using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class PaymentSettingsCreateModel : SuperAdminPageModel
{
    public PaymentSettingsCreateModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
