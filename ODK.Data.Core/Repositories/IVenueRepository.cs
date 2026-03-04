using ODK.Core.Venues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IVenueRepository : IReadWriteRepository<Venue, IVenueQueryBuilder>
{
    IDeferredQueryMultiple<Venue> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<Venue> GetByName(Guid chapterId, string name);
}