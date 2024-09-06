using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Users.ViewModels;

public class MemberPaymentsPageViewModel
{
    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<PaymentDto> Payments { get; init; }

    public required PlatformType Platform { get; init; }
}
