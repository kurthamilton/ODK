using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventTicketPaymentRepository : ReadWriteRepositoryBase<EventTicketPayment>, IEventTicketPaymentRepository
{
    public EventTicketPaymentRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventTicketPayment> GetConfirmedPayments(Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<EventTicketPayment> GetConfirmedPayments(Guid memberId, Guid eventId)
        => Set()
            .Where(x =>
                x.EventId == eventId &&
                x.Payment.MemberId == memberId &&
                x.Payment.PaidUtc != null)
            .DeferredMultiple();

    protected override IQueryable<EventTicketPayment> Set()
        => base.Set()
            .Include(x => x.Payment);
}