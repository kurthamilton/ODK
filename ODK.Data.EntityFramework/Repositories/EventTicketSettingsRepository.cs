using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventTicketSettingsRepository : WriteRepositoryBase<EventTicketSettings>, IEventTicketSettingsRepository
{
    public EventTicketSettingsRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<EventTicketSettings> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<EventTicketSettings> GetByEventIds(IEnumerable<Guid> eventIds) => Set()
        .Where(x => eventIds.Contains(x.EventId))
        .DeferredMultiple();
}