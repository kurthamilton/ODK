using ODK.Core.Venues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Venues;

namespace ODK.Data.Core.Repositories;

public interface IVenueRepository : IReadWriteRepository<Venue, IVenueQueryBuilder>
{
    /// <summary>
    /// The most recent record of a place, or null where none exists. A place keeps its id while its name
    /// or position change and each of those is its own venue, so this is the one a new link compares
    /// against - the latest, because a place that changes and changes back is a third record, not the
    /// first one again.
    /// </summary>
    IDeferredQuerySingleOrDefault<VenueWithLocationDto> GetLatestByExternalLocationId(string externalId);
}