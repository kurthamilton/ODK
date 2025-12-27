using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Users.ViewModels;

public class MemberPaymentsPageViewModel
{
    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<MemberPaymentsPageViewModelPayment> Payments { get; init; }

    public required PlatformType Platform { get; init; }
}
