using ODK.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class MemberTicketPurchaseViewModel
{
    public required decimal AmountPaid { get; init; }

    public required decimal AmountRemaining { get; init; }

    public required Member Member { get; init; }
}