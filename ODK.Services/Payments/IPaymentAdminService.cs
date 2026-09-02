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
    /// What to record against <paramref name="paymentId"/>, filled in from the payment where one is named
    /// and blank where it is not.
    /// </summary>
    Task<PaymentRefundCreateViewModel> GetPaymentRefundCreateViewModel(
        IMemberServiceRequest request, Guid? paymentId);

    /// <summary>
    /// The refunds recorded against this platform's payments, and what each left the group owing.
    /// </summary>
    Task<PaymentRefundsViewModel> GetPaymentRefundsViewModel(IMemberServiceRequest request);

    Task<ChapterPaymentsViewModel> GetPayments(IMemberChapterAdminServiceRequest request);

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

    /// <summary>
    /// Writes down a refund already made through the payment provider. It makes no refund and moves no
    /// money: what it does is stop a refund performed in the provider's dashboard being invisible here,
    /// and record what the group is left owing.
    /// </summary>
    Task<ServiceResult> RecordPaymentRefund(IMemberServiceRequest request, RecordPaymentRefundModel model);

    /// <summary>
    /// Undoes <see cref="IgnorePayment(IMemberServiceRequest, Guid)"/>.
    /// </summary>
    Task<ServiceResult> UnignorePayment(IMemberServiceRequest request, Guid paymentId);
}
