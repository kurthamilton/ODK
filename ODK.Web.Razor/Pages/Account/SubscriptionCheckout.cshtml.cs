namespace ODK.Web.Razor.Pages.Account;

public class SubscriptionCheckoutModel : OdkPageModel
{
    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid id)
    {
        SubscriptionId = id;
    }
}
