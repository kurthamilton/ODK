using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSubscriptionRepository : IWriteRepository<MemberSubscription>
{
    IDeferredQueryMultiple<MemberSubscription> GetByChapterId(Guid chapterId);
    IDeferredQueryMultiple<MemberSubscription> GetByChapterIds(IEnumerable<Guid> chapterIds);
    IDeferredQuerySingleOrDefault<MemberSubscription> GetByMemberId(Guid memberId, Guid chapterId);
}
