using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventTopicRepository : WriteRepositoryBase<EventTopic>, IEventTopicRepository
{
    public EventTopicRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventTopic> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredMultiple();
}
