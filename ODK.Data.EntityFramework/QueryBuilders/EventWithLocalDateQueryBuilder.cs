using Microsoft.EntityFrameworkCore;
using ODK.Data.Core.Events;
using ODK.Data.Core.QueryBuilders;
using EventEntity = ODK.Core.Events.Event;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class EventWithLocalDateQueryBuilder : QueryBuilder<EventWithLocalDateDto>, IEventWithLocalDateQueryBuilder
{
    public EventWithLocalDateQueryBuilder(DbContext context, IQueryable<EventWithLocalDateDto> query)
        : base(context, query)
    {
    }

    public IEventQueryBuilder Event()
    {
        var query = Query.Select(x => x.Event);
        return CreateQueryBuilder<IEventQueryBuilder, EventEntity>(context =>
            new EventQueryBuilder(context, query));
    }

    public IEventWithLocalDateQueryBuilder Filter(EventAdminFilter filter)
    {
        if (filter.FromDateLocal != null)
        {
            Query = Query
                .Where(x => x.DateLocal >= filter.FromDateLocal.Value);
        }

        if (filter.ToDateLocal != null)
        {
            Query = Query
                .Where(x => x.DateLocal <= filter.ToDateLocal.Value);
        }

        if (filter.VenueId != null)
        {
            Query = Query
                .Where(x => x.Event.VenueId == filter.VenueId.Value);
        }

        return this;
    }
}
