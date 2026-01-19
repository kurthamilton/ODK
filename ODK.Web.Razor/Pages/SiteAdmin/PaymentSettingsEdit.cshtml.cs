namespace ODK.Web.Razor.Pages.SiteAdmin;

public class PaymentSettingsEditModel : SiteAdminPageModel
{
    public PaymentSettingsEditModel()
    {
    }

    public Guid Id { get; private set; }

    public void OnGet(Guid id)
    {
        Id = id;
    }
}