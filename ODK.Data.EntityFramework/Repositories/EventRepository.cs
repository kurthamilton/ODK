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

    public IDeferredQueryMultiple<Event> GetByVenueId(Guid venueId) => Set()
        .Where(x => x.VenueId == venueId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Event> GetPublicEventsByChapterId(Guid chapterId, DateTime? after) => Set()
        .Where(x => x.ChapterId == chapterId && x.IsPublic)
        .ConditionalWhere(x => x.Date >= after, after != null)
        .OrderByDescending(x => x.Date)
        .DeferredMultiple();

    public IDeferredQuery<int> GetPastEventCountByChapterId(Guid chapterId)
        => GetPastEventQuery(chapterId, null)
            .DeferredCount();

    public IDeferredQueryMultiple<Event> GetRecentEventsByChapterId(Guid chapterId, int pageSize)
        => GetPastEventQuery(chapterId, pageSize)
            .OrderByDescending(x => x.Date)
            .DeferredMultiple();

    protected override IQueryable<Event> Set() => base.Set()
        .Include(x => x.TicketSettings)
        .ThenInclude(x => x!.Currency);

    private IQueryable<Event> GetPastEventQuery(Guid chapterId, int? pageSize)
    {
        var query = Set()
            .Where(x => x.ChapterId == chapterId && x.Date < DateTime.UtcNow && x.PublishedUtc != null);

        if (pageSize != null)
        {
            query = query
                .Take(pageSize.Value);
        }

        return query;
    }
}