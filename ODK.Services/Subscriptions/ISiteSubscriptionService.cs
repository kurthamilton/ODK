using ODK.Core.Chapters;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<ServiceResult> CancelMemberSiteSubscription(IMemberServiceRequest request);

    /// <summary>
    /// Moves every member whose site subscription has expired past the cooldown onto the platform's default
    /// plan, and tells them. Scoped to the plans this platform sells, and stateless: it asks only whether a
    /// subscription needs downgrading now, so how often it runs is the cron's business.
    /// </summary>
    Task DowngradeLapsedSubscriptions(IServiceRequest request);

    Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        IServiceRequest request, Chapter? chapter);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        IMemberServiceRequest request, Guid priceId, string returnPath);
}