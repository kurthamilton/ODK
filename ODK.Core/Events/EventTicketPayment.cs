using ODK.Core.Payments;

namespace ODK.Core.Events;

public class EventTicketPayment : IDatabaseEntity
{
    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public Payment Payment { get; set; } = null!;

    public Guid PaymentId { get; set; }
}