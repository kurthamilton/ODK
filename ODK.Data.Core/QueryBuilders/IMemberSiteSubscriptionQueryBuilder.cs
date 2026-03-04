using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSiteSubscriptionQueryBuilder
    : IDatabaseEntityQueryBuilder<MemberSiteSubscription, IMemberSiteSubscriptionQueryBuilder>
{
    IMemberSiteSubscriptionQueryBuilder Active();

    IMemberSiteSubscriptionQueryBuilder ForChapterOwner(Guid chapterId);

    IMemberSiteSubscriptionQueryBuilder ForMember(Guid memberId, PlatformType platform);

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    ISiteSubscriptionQueryBuilder SiteSubscription();

    IQueryBuilder<MemberSiteSubscriptionDto> ToMemberSiteSubscriptionDto();
}