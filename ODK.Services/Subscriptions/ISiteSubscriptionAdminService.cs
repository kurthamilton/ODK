using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionAdminService
{
    Task<ServiceResult> AddSiteSubscription(Guid currentMemberId, SiteSubscriptionCreateModel model);

    Task<ServiceResult> AddSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, 
        SiteSubscriptionPriceCreateModel model);

    Task DeleteSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId);

    Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(Guid currentMemberId);

    Task<SiteSubscriptionDto> GetSubscriptionDto(Guid currentMemberId, Guid siteSubscriptionId);

    Task<ServiceResult> UpdateSiteSubscription(Guid currentMemberId, Guid siteSubscriptionId, 
        SiteSubscriptionCreateModel model);
}
