using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventTicketsAdminPageViewModel : EventAdminPageViewModelBase
{
    public required ChapterPaymentSettings? PaymentSettings { get; init; }

    public required IReadOnlyCollection<EventTicketPurchase> Purchases { get; init; }
}
