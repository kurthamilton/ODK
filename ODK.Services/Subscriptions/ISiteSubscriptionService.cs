using ODK.Core.Members;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<MemberSiteSubscription?> GetMemberSiteSubscription(Guid memberId);

    Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto(Guid? memberId, Guid? chapterId);

    Task<ServiceResult> UpdateMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, 
        SiteSubscriptionFrequency frequency);
}
