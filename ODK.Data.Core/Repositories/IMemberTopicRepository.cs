using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberTopicRepository : IWriteRepository<MemberTopic>
{
    IDeferredQueryMultiple<MemberTopic> GetByMemberId(Guid memberId);

    int Merge(IEnumerable<MemberTopic> existing, Guid memberId, IEnumerable<Guid> topicIds);
}
