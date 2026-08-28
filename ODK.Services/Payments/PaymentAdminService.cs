using ODK.Data.Core;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public class PaymentAdminService : OdkAdminServiceBase, IPaymentAdminService
{
    private readonly IPaymentService _paymentService;

    public PaymentAdminService(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService)
        : base(unitOfWork)
    {
        _paymentService = paymentService;
    }

    public async Task<ChapterPaymentsViewModel> GetPayments(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var payments = await GetChapterAdminRestrictedContent(
            request,
            x => x.PaymentRepository.GetMemberDtosByChapterId(chapter.Id));

        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray()
        };
    }

    public async Task<ReconcilePaymentSettlementsResult> ReconcilePaymentSettlements(
        IMemberServiceRequest request)
    {
        var payments = await GetSiteAdminRestrictedContent(
            request,
            x => x.PaymentRepository.Query().Paid().WithoutSettlement().GetAll());

        /* The reference is what the provider is asked about, so a payment without one can only be counted.
           Payment settings are not required: a payment naming none has its account found by asking each of
           them, and one that no account holds is skipped by the job rather than filtered out here.

           One job per payment rather than one for all of them: each is a call to the provider that can fail
           on its own, and a batch would lose every payment after the first failure. They queue behind each
           other, so the provider is not asked for all of them at once either. */
        var queued = payments
            .Where(x => !string.IsNullOrEmpty(x.ExternalId))
            .ToArray();

        foreach (var payment in queued)
        {
            _paymentService.EnqueueResolvePaymentSettlementJob(payment.Id);
        }

        return ReconcilePaymentSettlementsResult.Successful(
            queued.Length, unidentifiable: payments.Count - queued.Length);
    }
}
