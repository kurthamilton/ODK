using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class PaymentSettingsEditModel : SuperAdminPageModel
{
    public PaymentSettingsEditModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public Guid Id { get; private set; }

    public void OnGet(Guid id)
    {
        Id = id;
    }
}
