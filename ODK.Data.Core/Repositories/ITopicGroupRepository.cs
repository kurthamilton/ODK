using ODK.Core.Topics;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ITopicGroupRepository : IReadWriteRepository<TopicGroup>
{
    IDeferredQueryMultiple<TopicGroup> GetAll();
}
