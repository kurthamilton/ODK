using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IMemberSiteSubscriptionRepository : IReadWriteRepository<MemberSiteSubscription, IMemberSiteSubscriptionQueryBuilder>
{
    IDeferredQueryMultiple<MemberSiteSubscriptionDto> GetAllChapterOwnerSubscriptionDtos(PlatformType platform);

    IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByMemberId(Guid memberId, PlatformType platform);

    IDeferredQuerySingleOrDefault<MemberSiteSubscriptionDto> GetDtoByMemberId(Guid memberId, PlatformType platform);
}