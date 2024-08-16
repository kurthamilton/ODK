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

    public IDeferredQuerySingle<Venue> GetByEventId(Guid eventId)
    {
        var venueIdQuery =
            from @event in Set<Event>().Where(x => x.Id == eventId)
            select @event.VenueId;

        var query =
            from venue in Set()
            where venueIdQuery.Contains(venue.Id)
            select venue;

        return query.DeferredSingle();
    }

    public IDeferredQueryMultiple<Venue> GetByEventIds(IEnumerable<Guid> eventIds)
    {
        var venueIdQuery =
            from @event in Set<Event>().Where(x => eventIds.Contains(x.Id))
            select @event.VenueId;

        var query =
            from venue in Set()
            where venueIdQuery.Contains(venue.Id)
            select venue;

        return query.DeferredMultiple();
    }
        

    public IDeferredQuerySingleOrDefault<Venue> GetByName(Guid chapterId, string name) => Set()
        .Where(x => x.ChapterId == chapterId && x.Name == name)
        .DeferredSingleOrDefault();
}
