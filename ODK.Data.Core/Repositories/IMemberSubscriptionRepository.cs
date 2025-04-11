using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSubscriptionRepository : IWriteRepository<MemberSubscription>
{    
    IDeferredQueryMultiple<MemberSubscription> GetByChapterId(Guid chapterId);
    IDeferredQuerySingleOrDefault<MemberSubscription> GetByMemberId(Guid memberId, Guid chapterId);
}
