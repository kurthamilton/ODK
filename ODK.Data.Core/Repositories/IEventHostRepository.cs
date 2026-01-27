using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventHostRepository : IReadWriteRepository<EventHost>
{
    IDeferredQueryMultiple<EventHost> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<EventHost> GetByEventShortcode(string shortcode);
}