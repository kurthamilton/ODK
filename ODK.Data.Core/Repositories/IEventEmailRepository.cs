using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventEmailRepository : IReadWriteRepository<EventEmail>
{
    IDeferredQuerySingleOrDefault<EventEmail> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<EventEmail> GetByEventIds(IEnumerable<Guid> eventIds);
}