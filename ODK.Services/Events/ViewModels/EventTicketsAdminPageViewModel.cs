using ODK.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventTicketsAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<EventTicketPurchase> Purchases { get; init; }
}