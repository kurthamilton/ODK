using ODK.Core.Chapters;
using ODK.Data.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

public class ChapterPaymentsViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether the group has a payment account it can take money through. False while onboarding is
    /// unfinished as well as before it starts, since neither state can be charged against.
    /// </summary>
    public required bool PaymentAccountEnabled { get; init; }

    public required IReadOnlyCollection<PaymentMemberDto> Payments { get; init; }
}