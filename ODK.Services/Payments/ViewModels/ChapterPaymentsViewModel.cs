using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

public class ChapterPaymentsViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<PaymentMemberDto> Payments { get; init; }

    public required PlatformType Platform { get; init; }
}