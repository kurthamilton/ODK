using ODK.Core.Subscriptions;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionAdminService
{
    Task<ServiceResult> AddSiteSubscription(Guid currentMemberId, SiteSubscriptionCreateModel model);

    Task<ServiceResult> AddSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, 
        SiteSubscriptionPriceCreateModel model);

    Task DeleteSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId);

    Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(Guid currentMemberId);

    Task<SiteSubscriptionViewModel> GetSubscriptionViewModel(Guid currentMemberId, Guid siteSubscriptionId);

    Task MakeDefault(Guid currentMemberId, Guid siteSubscriptionId);

    Task<ServiceResult> UpdateSiteSubscription(Guid currentMemberId, Guid siteSubscriptionId, 
        SiteSubscriptionCreateModel model);

    Task<ServiceResult> UpdateSiteSubscriptionEnabled(Guid currentMemberId, Guid siteSubscriptionId,
        bool enabled);
}
