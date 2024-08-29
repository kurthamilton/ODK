namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class SubscriptionEditModel : OdkGroupAdminPageModel
{
    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }
}
