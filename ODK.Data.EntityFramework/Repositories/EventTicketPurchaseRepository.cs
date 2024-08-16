using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventTicketPurchaseRepository : ReadWriteRepositoryBase<EventTicketPurchase>, IEventTicketPurchaseRepository
{
    public EventTicketPurchaseRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventTicketPurchase> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventTicketPurchase> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<EventTicketPurchase> GetByMemberId(Guid memberId, Guid eventId) => Set()
        .Where(x => x.MemberId == memberId && x.EventId == eventId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<EventTicketPurchase> GetByMemberId(Guid memberId, IEnumerable<Guid> eventIds) => Set()
        .Where(x => x.MemberId == memberId && eventIds.Contains(x.EventId))
        .DeferredMultiple();

    protected override IQueryable<EventTicketPurchase> Set() => base.Set()
        .Include(x => x.Member);
}
