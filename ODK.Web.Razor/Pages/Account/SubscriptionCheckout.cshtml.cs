namespace ODK.Web.Razor.Pages.Account;

public class SubscriptionCheckoutModel : OdkSiteAccountPageModel
{
    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid id)
    {
        SubscriptionId = id;
    }
}