using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IMemberSubscriptionRepository : IWriteRepository<MemberSubscription>
{
    void AddMemberSubscriptionRecord(MemberSubscriptionRecord record);
    IDeferredQueryMultiple<MemberSubscription> GetByChapterId(Guid chapterId);
    IDeferredQuerySingle<MemberSubscription> GetByMemberId(Guid memberId, Guid chapterId);
    IDeferredQuerySingleOrDefault<MemberSubscription> GetByMemberIdOrDefault(Guid memberId, Guid chapterId);
}
