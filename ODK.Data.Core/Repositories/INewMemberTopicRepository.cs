using ODK.Core.Topics;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface INewMemberTopicRepository : IReadWriteRepository<NewMemberTopic>
{
    public IDeferredQueryMultiple<NewMemberTopic> GetAll();

    public IDeferredQueryMultiple<NewMemberTopic> GetByMemberId(Guid memberId);
}
