using ODK.Core.Members;

namespace ODK.Core.Events;

public class EventTicketPurchase : IDatabaseEntity
{
    public decimal? DepositPaid { get; set; }

    public DateTime? DepositPurchasedUtc { get; set; }

    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public Member Member { get; set; } = null!;

    public Guid MemberId { get; set; }

    public DateTime? PurchasedUtc { get; set; }

    public decimal TotalPaid { get; set; }
}
