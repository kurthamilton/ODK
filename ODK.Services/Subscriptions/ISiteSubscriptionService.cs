using ODK.Core.Members;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{    
    Task<ServiceResult> ConfirmMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, string externalId);

    Task<MemberSiteSubscription?> GetMemberSiteSubscription(Guid memberId);

    Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto(Guid? memberId, Guid? chapterId);

    Task SyncExpiredSubscriptions();

    Task<ServiceResult> UpdateMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, 
        SiteSubscriptionFrequency frequency);
}
