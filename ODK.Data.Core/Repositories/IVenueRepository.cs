using ODK.Core.Venues;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IVenueRepository : IReadWriteRepository<Venue>
{
    IDeferredQueryMultiple<Venue> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<Venue> GetByChapterId(Guid chapterId, IEnumerable<Guid> venueIds);

    IDeferredQuerySingle<Venue> GetByEventId(Guid eventId);

    IDeferredQuerySingleOrDefault<Venue> GetByName(Guid chapterId, string name);

    IDeferredQuerySingleOrDefault<Venue> GetPublicVenue(Guid id);
}
