using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class EventRepository : ReadWriteRepositoryBase<Event, IEventQueryBuilder>, IEventRepository
{
    public EventRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .OrderByDescending(x => x.DateUtc)
            .GetAll();

    public IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, DateTime after)
        => Query()
            .ForChapter(chapterId)
            .After(after)
            .OrderByDescending(x => x.DateUtc)
            .GetAll();

    public IDeferredQuerySingle<Event> GetByShortcode(string shortcode)
        => Query()
            .ForShortcode(shortcode)
            .GetSingle();

    public IDeferredQueryMultiple<Event> GetByVenueId(Guid venueId)
        => Query()
            .ForVenue(venueId)
            .GetAll();

    public IDeferredQuery<int> GetCountByChapterId(Guid chapterId, string? venueSlug, DateTime? fromUtc, DateTime? toUtcExclusive)
        => ApplyFilter(Query().ForChapter(chapterId), venueSlug, fromUtc, toUtcExclusive)
            .Count();

    public IDeferredQuery<int> GetPastEventCountByChapterId(Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .Past()
            .Count();

    public IDeferredQueryMultiple<Event> GetRecentEventsByChapterId(Guid chapterId, int pageSize)
        => Query()
            .ForChapter(chapterId)
            .Past()
            .OrderByDescending(x => x.DateUtc)
            .Take(pageSize)
            .GetAll();

    public IDeferredQueryMultiple<EventSummaryDto> GetSummariesByChapterId(
        Guid chapterId, string? venueSlug, DateTime? fromUtc, DateTime? toUtcExclusive, PageFilter pageFilter)
        => ApplyFilter(Query().ForChapter(chapterId), venueSlug, fromUtc, toUtcExclusive)
            .Summary()
            .OrderByDescending(x => x.Event.DateUtc)
            .Page(pageFilter)
            .GetAll();

    public IDeferredQueryMultiple<Event> GetUpcoming(Guid chapterId, int pageSize)
        => Query()
            .ForChapter(chapterId)
            .After(DateTime.UtcNow)
            .OrderBy(x => x.DateUtc)
            .Page(1, pageSize)
            .GetAll();

    public override IEventQueryBuilder Query() => CreateQueryBuilder<IEventQueryBuilder, Event>(
        context => new EventQueryBuilder(context));

    public IDeferredQuery<bool> ShortcodeExists(string shortcode)
        => Query()
            .ForShortcode(shortcode)
            .Any();

    // Date bounds are UTC instants resolved from the chapter's timezone by the service; the query
    // stays a simple (index-friendly) UTC range.
    private static IEventQueryBuilder ApplyFilter(
        IEventQueryBuilder query, string? venueSlug, DateTime? fromUtc, DateTime? toUtcExclusive)
    {
        if (!string.IsNullOrEmpty(venueSlug))
        {
            query = query.ForVenueSlug(venueSlug);
        }

        if (fromUtc != null)
        {
            query = query.OnOrAfter(fromUtc.Value);
        }

        if (toUtcExclusive != null)
        {
            query = query.Before(toUtcExclusive.Value);
        }

        return query;
    }
}