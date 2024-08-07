using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionAdminService
{
    Task<ServiceResult> AddSiteSubscription(Guid currentMemberId, SiteSubscriptionCreateModel model);

    Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(Guid currentMemberId);
}
