using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Payments.ViewModels;

public class ChapterPaymentsViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterPaymentSettings? PaymentSettings { get; init; }

    public required IReadOnlyCollection<PaymentMemberDto> Payments { get; init; }

    public required PlatformType Platform { get; init; }
}
