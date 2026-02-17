using ODK.Data.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class MemberTicketPurchaseViewModel
{
    public required decimal AmountPaid { get; init; }

    public required decimal AmountRemaining { get; init; }

    public required MemberWithAvatarDto Member { get; init; }
}