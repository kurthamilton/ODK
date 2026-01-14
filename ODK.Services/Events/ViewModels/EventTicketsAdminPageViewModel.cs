namespace ODK.Services.Events.ViewModels;

public class EventTicketsAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<MemberTicketPurchaseViewModel> Payments { get; init; }
}