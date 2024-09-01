using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSiteSubscriptionRepository : IWriteRepository<MemberSiteSubscription>
{
    IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<MemberSiteSubscription> GetByMemberId(Guid memberId, PlatformType platform);

    IDeferredQueryMultiple<MemberSiteSubscription> GetByMemberId(Guid memberId);

    IDeferredQueryMultiple<MemberSiteSubscription> GetExpired();
}
