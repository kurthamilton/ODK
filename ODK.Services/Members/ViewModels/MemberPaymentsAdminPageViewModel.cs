using ODK.Core.Chapters;
using ODK.Data.Core.Payments;

namespace ODK.Services.Members.ViewModels;

public class MemberPaymentsAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required ChapterPaymentSettings ChapterPaymentSettings { get; init; }

    public required IReadOnlyCollection<PaymentDto> Payments { get; init; }
}
