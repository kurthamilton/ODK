using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventRepository : ReadWriteRepositoryBase<Event>, IEventRepository
{
    public EventRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId) => GetByChapterId(chapterId, null);

    public IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, DateTime? after) => Set()
        .Where(x => x.ChapterId == chapterId)
        .ConditionalWhere(x => x.Date >= after, after != null)
        .OrderByDescending(x => x.Date)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, int page, int pageSize) => Set()
        .Where(x => x.ChapterId == chapterId)
        .OrderByDescending(x => x.Date)
        .Page(page, pageSize)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Event> GetByIds(IEnumerable<Guid> ids) => Set()
        .Where(x => ids.Contains(x.Id))
        .DeferredMultiple();

    public IDeferredQueryMultiple<Event> GetByVenueId(Guid venueId) => Set()
        .Where(x => x.VenueId == venueId)
        .DeferredMultiple();    

    public IDeferredQueryMultiple<Event> GetPublicEventsByChapterId(Guid chapterId, DateTime? after) => Set()
        .Where(x => x.ChapterId == chapterId && x.IsPublic)
        .ConditionalWhere(x => x.Date >= after, after != null)
        .OrderByDescending(x => x.Date)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Event> GetRecentEventsByChapterId(Guid chapterId, int pageSize) => Set()
        .Where(x => x.ChapterId == chapterId && x.Date < DateTime.UtcNow && x.PublishedUtc != null)
        .OrderByDescending(x => x.Date)
        .Take(pageSize)
        .DeferredMultiple();

    protected override IQueryable<Event> Set() => base.Set()
        .Include(x => x.TicketSettings);
}
