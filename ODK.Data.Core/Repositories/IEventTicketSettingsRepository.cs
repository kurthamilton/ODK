using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventTicketSettingsRepository : IWriteRepository<EventTicketSettings>
{
    IDeferredQuerySingleOrDefault<EventTicketSettings> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<EventTicketSettings> GetByEventIds(IEnumerable<Guid> eventIds);
}
