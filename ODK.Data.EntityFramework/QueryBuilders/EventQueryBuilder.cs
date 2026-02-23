using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.Events;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class EventQueryBuilder : DatabaseEntityQueryBuilder<Event, IEventQueryBuilder>, IEventQueryBuilder
{
    public EventQueryBuilder(OdkContext context) 
        : base(context, BaseQuery(context))
    {
    }

    protected override IEventQueryBuilder Builder => this;

    public IEventQueryBuilder After(DateTime date)
    {
        Query = Query.Where(x => x.Date > date);
        return this;
    }

    public IEventQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IEventQueryBuilder ForShortcode(string shortcode)
    {
        Query = Query.Where(x => x.Shortcode == shortcode);
        return this;
    }

    public IEventQueryBuilder ForVenue(Guid venueId)
    {
        Query = Query.Where(x => x.VenueId == venueId);
        return this;
    }

    public IEventQueryBuilder Past()
    {
        Query = Query.Where(x => x.Date < DateTime.UtcNow);
        return this;
    }

    public IEventQueryBuilder Public()
    {
        Query = Query.Where(x => x.IsPublic);
        return this;
    }

    public IQueryBuilder<EventSummaryDto> Summary()
    {
        var query =
            from @event in Query
            from venue in Set<Venue>()
                .Where(x => x.Id == @event.VenueId)
            from email in Set<EventEmail>()
                .Where(x => x.EventId == @event.Id)
                .DefaultIfEmpty()
            select new EventSummaryDto
            {
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
                },
                Venue = venue
            };

        return ProjectTo(query);
    }

    private static IQueryable<Event> BaseQuery(OdkContext context)
        => context.Set<Event>()
            .Include(x => x.TicketSettings)
            .ThenInclude(x => x!.Currency);
}
