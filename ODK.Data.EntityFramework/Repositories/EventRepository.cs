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
            .OrderByDescending(x => x.Date)
            .GetAll();

    public IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, DateTime after)
        => Query()
            .ForChapter(chapterId)
            .After(after)
            .OrderByDescending(x => x.Date)
            .GetAll();

    public IDeferredQuerySingle<Event> GetByShortcode(string shortcode)
        => Query()
            .ForShortcode(shortcode)
            .GetSingle();

    public IDeferredQueryMultiple<Event> GetByVenueId(Guid venueId)
        => Query()
            .ForVenue(venueId)
            .GetAll();

    public IDeferredQuery<int> GetCountByChapterId(Guid chapterId, EventAdminFilter filter)
        => Query()
            .ForChapter(chapterId)
            .Filter(filter)
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
            .OrderByDescending(x => x.Date)
            .Take(pageSize)
            .GetAll();

    public IDeferredQueryMultiple<EventSummaryDto> GetSummariesByChapterId(
        Guid chapterId, EventAdminFilter filter, PageFilter pageFillter)
        => Query()
            .ForChapter(chapterId)
            .Filter(filter)
            .Summary()
            .OrderByDescending(x => x.Event.Date)
            .Page(pageFillter)
            .GetAll();

    public IDeferredQueryMultiple<Event> GetUpcoming(Guid chapterId, int pageSize)
        => Query()
            .ForChapter(chapterId)
            .After(DateTime.UtcNow)
            .OrderBy(x => x.Date)
            .Page(1, pageSize)
            .GetAll();

    public override IEventQueryBuilder Query() => CreateQueryBuilder<IEventQueryBuilder, Event>(
        context => new EventQueryBuilder(context));

    public IDeferredQuery<bool> ShortcodeExists(string shortcode)
        => Query()
            .ForShortcode(shortcode)
            .Any();

    public override void Update(Event entity)
    {
        // do not include Currency in the update
        var clone = entity.Clone();
        clone.TicketSettings?.Currency = null!;

        base.Update(entity);
    }
}