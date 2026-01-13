using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Events;

public class EventTicketPurchaseViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterPaymentSettings ChapterPaymentSettings { get; init; }

    public required Guid CurrentMemberId { get; init; }

    public required Event Event { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<EventTicketPurchase> TicketPurchases { get; init; }

    public required int? TicketsLeft { get; init; }
}
