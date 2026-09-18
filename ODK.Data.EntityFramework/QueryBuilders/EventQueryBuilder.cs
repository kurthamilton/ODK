using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.Events;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class EventQueryBuilder : DatabaseEntityQueryBuilder<Event, IEventQueryBuilder>, IEventQueryBuilder
{
    public EventQueryBuilder(DbContext context)
        : base(context, BaseQuery(context))
    {
    }

    public EventQueryBuilder(DbContext context, IQueryable<Event> query)
        : base(context, query)
    {
    }

    protected override IEventQueryBuilder Builder => this;

    public IEventQueryBuilder After(DateTime date)
    {
        Query = Query.Where(x => x.DateUtc > date);
        return this;
    }

    public IEventQueryBuilder Before(DateTime date)
    {
        Query = Query.Where(x => x.DateUtc < date);
        return this;
    }

    public IEventQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IEventQueryBuilder ForChapters(IEnumerable<Guid> chapterIds)
    {
        Query = Query.Where(x => chapterIds.Contains(x.ChapterId));
        return this;
    }

    public IEventQueryBuilder ForChapterVenue(Guid chapterVenueId)
    {
        Query = Query.Where(x => x.ChapterVenueId == chapterVenueId);
        return this;
    }

    public IEventQueryBuilder ForShortcode(string shortcode)
    {
        Query = Query.Where(x => x.Shortcode == shortcode);
        return this;
    }

    /// <summary>
    /// Every chapter's events at a venue, reached through the links - a venue is site-level and an event
    /// holds the link rather than the venue.
    /// </summary>
    public IEventQueryBuilder ForVenue(Guid venueId)
    {
        Query =
            from @event in Query
            from chapterVenue in Set<ChapterVenue>()
                .Where(x => x.Id == @event.ChapterVenueId && x.VenueId == venueId)
            select @event;

        return this;
    }

    public IEventQueryBuilder ForVenueSlug(string slug)
    {
        Query =
            from @event in Query
            from chapterVenue in Set<ChapterVenue>()
                .Where(x => x.Id == @event.ChapterVenueId)
            from venue in Set<Venue>()
                .Where(x => x.Id == chapterVenue.VenueId && x.Slug == slug)
            select @event;

        return this;
    }

    public IEventQueryBuilder OnOrAfter(DateTime date)
    {
        Query = Query.Where(x => x.DateUtc >= date);
        return this;
    }

    public IEventQueryBuilder Past()
    {
        Query = Query.Where(x => x.DateUtc < DateTime.UtcNow);
        return this;
    }

    public IEventQueryBuilder Public()
    {
        Query = Query.Where(x => x.IsPublic);
        return this;
    }

    public IQueryBuilder<EventPublicationDto> Publication()
    {
        var query = Query
            .Where(x => x.PublishedUtc != null)
            .Select(x => new EventPublicationDto
            {
                ChapterId = x.ChapterId,
                PublishedUtc = x.PublishedUtc!.Value,
                Shortcode = x.Shortcode
            });

        return ProjectTo(query);
    }

    public IEventQueryBuilder Published()
    {
        Query = Query.Where(x => x.PublishedUtc != null);
        return this;
    }

    public IQueryBuilder<EventSummaryDto> Summary()
    {
        var query =
            from @event in Query
            from chapterVenue in Set<ChapterVenue>()
                .Include(x => x.Venue)
                .Where(x => x.Id == @event.ChapterVenueId)
            from email in Set<EventEmail>()
                .Where(x => x.EventId == @event.Id)
                .DefaultIfEmpty()
            select new EventSummaryDto
            {
                ChapterVenue = chapterVenue,
                Email = email,
                Event = @event,
                Invites = new EventInviteSummaryDto
                {
                    EventId = @event.Id,
                    Sent = Set<EventInvite>()
                        .Where(x => x.EventId == @event.Id)
                        .Count()
                },
                Responses = new EventResponseSummaryDto
                {
                    EventId = @event.Id,
                    Maybe = Set<EventResponse>()
                        .Where(x => x.EventId == @event.Id && x.Type == EventResponseType.Maybe)
                        .Count(),
                    No = Set<EventResponse>()
                        .Where(x => x.EventId == @event.Id && x.Type == EventResponseType.No)
                        .Count(),
                    Yes = Set<EventResponse>()
                        .Where(x => x.EventId == @event.Id && x.Type == EventResponseType.Yes)
                        .Count()
                }
            };

        return ProjectTo(query);
    }

    public IVenueQueryBuilder Venue()
    {
        var query =
            from @event in Query
            from chapterVenue in Set<ChapterVenue>()
                .Where(x => x.Id == @event.ChapterVenueId)
            from venue in Set<Venue>()
                .Where(x => x.Id == chapterVenue.VenueId)
            select venue;
        return CreateQueryBuilder<IVenueQueryBuilder, Venue>(
            context => new VenueQueryBuilder(context, query));
    }

    public IQueryBuilder<EventWithVenueDto> WithVenue()
    {
        var query =
            from @event in Query
            from chapterVenue in Set<ChapterVenue>()
                .Include(x => x.Venue)
                .Where(x => x.Id == @event.ChapterVenueId)
            select new EventWithVenueDto
            {
                ChapterVenue = chapterVenue,
                Event = @event
            };

        return ProjectTo(query);
    }

    private static IQueryable<Event> BaseQuery(DbContext context)
        => context.Set<Event>()
            .Include(x => x.TicketSettings)
            .ThenInclude(x => x!.Currency);
}