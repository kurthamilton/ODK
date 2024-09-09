using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventTopicRepository : IWriteRepository<EventTopic>
{
    IDeferredQueryMultiple<EventTopic> GetByEventId(Guid eventId);
}
