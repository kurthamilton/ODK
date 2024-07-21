using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class EventEmailRepository : ReadWriteRepositoryBase<EventEmail>, IEventEmailRepository
{
    public EventEmailRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<EventEmail> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<EventEmail> GetByEventIds(IEnumerable<Guid> eventIds) => Set()
        .Where(x => eventIds.Contains(x.EventId))
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventEmail> GetScheduled() => Set()
        .Where(x => x.ScheduledDate != null && x.SentDate == null)
        .DeferredMultiple();
}
