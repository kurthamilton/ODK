using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IPaymentAdminService
{
    Task<ChapterPaymentsViewModel> GetPayments(IMemberChapterAdminServiceRequest request);

    /// <summary>
    /// Queues a settlement read for every paid payment that has never had one, so that what each party
    /// received is filled in for payments taken before it was recorded, or whose read failed at the time.
    /// </summary>
    Task<ReconcilePaymentSettlementsResult> ReconcilePaymentSettlements(IMemberServiceRequest request);
}
