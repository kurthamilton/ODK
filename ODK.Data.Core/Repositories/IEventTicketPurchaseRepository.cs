using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventTicketPurchaseRepository : IReadWriteRepository<EventTicketPurchase>
{
    IDeferredQueryMultiple<EventTicketPurchase> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<EventTicketPurchase> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<EventTicketPurchase> GetByMemberId(Guid memberId, Guid eventId);

    IDeferredQueryMultiple<EventTicketPurchase> GetByMemberId(Guid memberId, IEnumerable<Guid> eventIds);
}
