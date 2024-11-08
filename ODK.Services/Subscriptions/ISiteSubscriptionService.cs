using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{    
    Task<ServiceResult> ConfirmMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, string externalId);

    Task<MemberSiteSubscription?> GetMemberSiteSubscription(Guid memberId);

    Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(Guid? memberId);

    Task SyncExpiredSubscriptions();

    Task<ServiceResult> UpdateMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, 
        SiteSubscriptionFrequency frequency);
}
