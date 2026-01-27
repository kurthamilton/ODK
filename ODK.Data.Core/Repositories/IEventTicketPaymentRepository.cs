using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventTicketPaymentRepository : IReadWriteRepository<EventTicketPayment>
{
    IDeferredQueryMultiple<EventTicketPayment> GetConfirmedPayments(Guid eventId);

    IDeferredQueryMultiple<EventTicketPayment> GetConfirmedPayments(Guid memberId, Guid eventId);

    IDeferredQueryMultiple<EventTicketPayment> GetConfirmedPayments(Guid memberId, string eventShortcode);
}