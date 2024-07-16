using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class VenueRepository : ReadWriteRepositoryBase<Venue>, IVenueRepository
{
    public VenueRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Venue> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Venue> GetByChapterId(Guid chapterId, IEnumerable<Guid> venueIds) => Set()
        .Where(x => x.ChapterId == chapterId && venueIds.Contains(x.Id))
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Venue> GetByName(Guid chapterId, string name) => Set()
        .Where(x => x.ChapterId == chapterId && x.Name == name)
        .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<Venue> GetPublicVenue(Guid id)
    {
        var query =
            from venue in Set()
            from @event in Set<Event>()
            where @event.VenueId == venue.Id
                && venue.Id == id
                && @event.IsPublic
            select venue;
        return query.DeferredSingleOrDefault();


    }
}
