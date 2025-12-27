using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin.Subscriptions;

public class SubscriptionModel : SuperAdminPageModel
{
    public SubscriptionModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid id)
    {
        SubscriptionId = id;
    }
}
