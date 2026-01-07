using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

public class ChapterReconciliationsViewModel
{
    public required ChapterPaymentSettings PaymentSettings { get; init; }

    public required IReadOnlyCollection<PaymentMemberDto> PendingReconciliations { get; init; }

    public required decimal PendingReconciliationsAmount { get; init; }

    public required IReadOnlyCollection<PaymentReconciliation> Reconciliations { get; init; }
}
