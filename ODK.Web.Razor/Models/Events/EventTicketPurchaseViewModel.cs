using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventTicketPurchaseViewModel
{
    public required decimal AmountPaid { get; init; }

    public required decimal AmountRemaining { get; init; }

    public required Chapter Chapter { get; init; }

    public required Event Event { get; init; }

    public required int? TicketsLeft { get; init; }
}