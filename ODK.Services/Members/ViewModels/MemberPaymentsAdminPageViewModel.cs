using ODK.Core.Payments;

namespace ODK.Services.Members.ViewModels;

public class MemberPaymentsAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required IReadOnlyCollection<Payment> Payments { get; init; }
}