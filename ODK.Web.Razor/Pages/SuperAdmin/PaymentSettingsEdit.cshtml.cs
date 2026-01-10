namespace ODK.Web.Razor.Pages.SuperAdmin;

public class PaymentSettingsEditModel : SuperAdminPageModel
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