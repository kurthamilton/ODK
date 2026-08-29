using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteSubscriptionFormViewModel : SiteSubscriptionFormSubmitViewModel
{
    public required Guid? SiteSubscriptionId { get; init; }

    public required IReadOnlyCollection<SiteSubscription> SiteSubscriptions { get; init; }
}