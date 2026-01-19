namespace ODK.Web.Razor.Pages.SiteAdmin.Subscriptions;

public class SubscriptionModel : SiteAdminPageModel
{
    public SubscriptionModel()
    {
    }

    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid id)
    {
        SubscriptionId = id;
    }
}