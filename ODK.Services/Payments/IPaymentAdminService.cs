using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IPaymentAdminService
{
    /// <summary>
    /// The payments waiting on something the provider has yet to be asked for, and those ruled out.
    /// </summary>
    Task<PaymentReconciliationViewModel> GetPaymentReconciliationViewModel(IMemberServiceRequest request);

    /// <summary>
    /// The refunds recorded against this platform's payments, and what each left the group owing.
    /// </summary>
    Task<PaymentRefundsViewModel> GetPaymentRefundsViewModel(IMemberServiceRequest request);

    Task<ChapterPaymentsViewModel> GetPayments(IMemberChapterAdminServiceRequest request);

    /// <summary>
    /// Every payment taken for the platform, whichever group it was for. The same rows as a group's own
    /// payments page, with the group named and no group's payments left out.
    /// </summary>
    Task<SitePaymentsViewModel> GetSitePayments(IMemberServiceRequest request);

    /// <summary>
    /// Tells reconciliation to ignore a payment, for one nothing the provider can be asked will ever
    /// answer for. An ignored payment leaves the pending tables and is skipped by the job itself, so a
    /// read queued directly respects it too.
    /// </summary>
    Task<ServiceResult> IgnorePayment(IMemberServiceRequest request, Guid paymentId);

    /// <inheritdoc cref="IgnorePayment(IMemberServiceRequest, Guid)"/>
    /// <remarks>
    /// Acts on the ids given and nothing else, so a bulk action covers exactly the rows that were on the
    /// page. Any that are no longer outstanding are skipped and said to have been.
    /// </remarks>
    Task<ServiceResult> IgnorePayments(
        IMemberServiceRequest request, IReadOnlyCollection<Guid> paymentIds);

    /// <summary>
    /// Queues the read for one payment. Fails where that payment is not one
    /// <see cref="GetPaymentReconciliationViewModel(IMemberServiceRequest)"/> would list, so a row action
    /// can never reach further than the page showing it.
    /// </summary>
    Task<ServiceResult> ReconcilePayment(IMemberServiceRequest request, Guid paymentId);

    /// <summary>
    /// Queues the read for each of the given payments. Acts on the ids given and nothing else, so a bulk
    /// action covers exactly the rows that were on the page rather than whatever is pending by the time the
    /// form arrives. Safe to run repeatedly: the job writes nothing it has written before.
    /// </summary>
    Task<ServiceResult> ReconcilePayments(
        IMemberServiceRequest request, IReadOnlyCollection<Guid> paymentIds);

    Task<ServiceResult> RefundPayment(IMemberServiceRequest request, Guid paymentId, RefundPaymentModel model);

    /// <summary>
    /// Undoes <see cref="IgnorePayment(IMemberServiceRequest, Guid)"/>.
    /// </summary>
    Task<ServiceResult> UnignorePayment(IMemberServiceRequest request, Guid paymentId);
}
