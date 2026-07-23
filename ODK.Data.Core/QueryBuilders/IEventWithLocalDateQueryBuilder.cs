using ODK.Data.Core.Events;

namespace ODK.Data.Core.QueryBuilders;

public interface IEventWithLocalDateQueryBuilder : IQueryBuilder<EventWithLocalDateDto>
{
    IEventQueryBuilder Event();

    IEventWithLocalDateQueryBuilder Filter(EventAdminFilter filter);
}
