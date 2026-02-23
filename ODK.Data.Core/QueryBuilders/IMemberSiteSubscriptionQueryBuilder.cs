using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSiteSubscriptionQueryBuilder
    : IDatabaseEntityQueryBuilder<MemberSiteSubscription, IMemberSiteSubscriptionQueryBuilder>
{
    IMemberSiteSubscriptionQueryBuilder Active();

    IMemberSiteSubscriptionQueryBuilder ForChapterOwner(Guid chapterId);

    IMemberSiteSubscriptionQueryBuilder ForMember(Guid memberId, PlatformType platform);

    IQueryBuilder<SiteSubscription> SiteSubscription();

    IQueryBuilder<MemberSiteSubscriptionDto> ToMemberSiteSubscriptionDto();
}