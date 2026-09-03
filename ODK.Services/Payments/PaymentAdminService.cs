using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Data.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public class PaymentAdminService : OdkAdminServiceBase, IPaymentAdminService
{
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentAdminService(IUnitOfWork unitOfWork, IPaymentService paymentService)
        : base(unitOfWork)
    {
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentReconciliationViewModel> GetPaymentReconciliationViewModel(
        IMemberServiceRequest request)
    {
        var (unsettled, untransferred, unconfirmed, ignored) = await GetSiteAdminRestrictedContent(
            request,
            x => UnsettledPayments(request, x).WithReconciliation().GetAll(),
            x => UntransferredPayments(request, x).WithReconciliation().GetAll(),
            x => UnconfirmedRefundPayments(request, x).WithReconciliation().GetAll(),
            x => IgnoredPayments(request, x).WithReconciliation().GetAll());

        var (chapterNames, payableChapterIds) = await GetChapters(
            request.Platform,
            unsettled.Concat(untransferred).Concat(unconfirmed).Concat(ignored).Select(x => x.Payment));

        var items = unsettled
            .Select(x => ToItem(x, chapterNames, ToUnsettledType(x.Payment, payableChapterIds)))
            .Concat(untransferred
                .Select(x => ToItem(x, chapterNames, PaymentReconciliationType.TransferRecord)))
            .Concat(unconfirmed
                .Select(x => ToItem(x, chapterNames, PaymentReconciliationType.RefundRecord)));

        return new PaymentReconciliationViewModel
        {
            /* An ignored payment keeps the kind it would have been, so the page can still say what
               un-ignoring it would do. */
            Ignored = ignored
                .Select(x => ToItem(x, chapterNames, ToIgnoredType(x.Payment, payableChapterIds)))
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray(),
            Payments = items
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray(),
            TimeZone = request.CurrentMember.TimeZone
        };
    }

    public async Task<PaymentRefundsViewModel> GetPaymentRefundsViewModel(IMemberServiceRequest request)
    {
        var payments = await GetSiteAdminRestrictedContent(
            request,
            x => x.PaymentRepository
                .Query()
                .ForEnvironment(request.Environment)
                .ForPlatform(request.Platform)
                .GetAll());

        var paymentsById = payments.ToDictionary(x => x.Id);

        var refunds = await _unitOfWork.PaymentRefundRepository
            .Query()
            .ForPayments(paymentsById.Keys)
            .GetAll()
            .Run();

        var chapterNames = (await GetChapters(
            request.Platform,
            refunds.Select(x => paymentsById[x.PaymentId]))).Names;

        var refundIds = refunds.Select(x => x.Id).ToArray();

        /* What is still owed lives on the adjustment, not on the refund: a later transfer pays it down, and
           a figure worked out from the refund alone would go on claiming a debt already collected. */
        var (adjustments, reversals) = await _unitOfWork.Run(
            x => x.ChapterPaymentAdjustmentRepository.Query().ForRefunds(refundIds).GetAll(),
            x => x.PaymentTransferReversalRepository.Query().ForRefunds(refundIds).GetAll());

        var outstandingByRefund = adjustments
            .ToDictionary(x => x.PaymentRefundId!.Value, x => -x.Outstanding());

        var reversedByRefund = reversals
            .GroupBy(x => x.PaymentRefundId)
            .ToDictionary(x => x.Key, x => x.Sum(r => r.ActualAmount ?? r.Amount));

        return new PaymentRefundsViewModel
        {
            Refunds = refunds
                .Select(x => ToRefundItem(
                    x, paymentsById[x.PaymentId], chapterNames, outstandingByRefund, reversedByRefund))
                .OrderByDescending(x => x.Refund.RequestedUtc)
                .ToArray(),
            TimeZone = request.CurrentMember.TimeZone
        };
    }

    public async Task<ChapterPaymentsViewModel> GetPayments(
        IMemberChapterAdminServiceRequest request)
    {
        var (environment, chapter) = (request.Environment, request.Chapter);

        var (payments, paymentAccount) = await GetChapterAdminRestrictedContent(
            request,
            x => x.PaymentRepository.Query()
                .ForEnvironment(environment)
                .ForChapter(chapter.Id)
                .WithMember()
                .GetAll(),
            x => x.ChapterPaymentAccountRepository.Query()
                .ForEnvironment(environment)
                .ForChapter(chapter.Id)
                .GetSingleOrDefault());

        /* A second round trip, because both are found by the payments the first one returned. Always
           loaded rather than only for a site admin: what a payment has been refunded, and what share of it
           reached the group, is the group's own business. */
        var paymentIds = payments.Select(p => p.Payment.Id).ToArray();

        var (refunds, transfers) = await GetChapterAdminRestrictedContent(
            request,
            x => x.PaymentRefundRepository
                .Query()
                .ForPayments(paymentIds)
                .Live()
                .GetAll(),
            x => x.PaymentTransferRepository
                .Query()
                .ForPayments(paymentIds)
                .GetAll());

        var refundsByPayment = refunds
            .GroupBy(x => x.PaymentId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var transfersByPayment = transfers.ToDictionary(x => x.PaymentId);

        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            PaymentAccountEnabled = paymentAccount?.SetupComplete() == true,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .Select(x => ToChapterPaymentItem(x, refundsByPayment, transfersByPayment))
                .ToArray(),
            ViewedBySiteAdmin = request.CurrentMember.SiteAdmin
        };
    }

    public async Task<ServiceResult> IgnorePayment(
        IMemberServiceRequest request, Guid paymentId)
        => await SetPaymentIgnored(request, paymentId, ignored: true);

    public async Task<ServiceResult> IgnorePayments(
        IMemberServiceRequest request, IReadOnlyCollection<Guid> paymentIds)
    {
        var payments = await GetPendingPayments(request, paymentIds);

        if (payments.Count == 0)
        {
            return ServiceResult.Failure("None of those payments are waiting to be reconciled");
        }

        var reconciliations = (await _unitOfWork.PaymentReconciliationRepository
                .Query()
                .ForPayments(payments.Select(x => x.Id))
                .GetAll()
                .Run())
            .ToDictionary(x => x.PaymentId);

        foreach (var payment in payments)
        {
            SetIgnored(payment, reconciliations.GetValueOrDefault(payment.Id), ignored: true);
        }

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful(
            ToOutcomeMessage("Ignored", payments.Count, paymentIds.Count));
    }

    public async Task<ServiceResult> ReconcilePayment(IMemberServiceRequest request, Guid paymentId)
    {
        /* Looked up through the same definition of pending the page lists, so a row action can only reach
           what the page could show it - a payment on another platform, in another environment, or with
           nothing left to read is simply not found. */
        var (unsettled, untransferred, unconfirmed) = await GetSiteAdminRestrictedContent(
            request,
            x => UnsettledPayments(request, x).ById(paymentId).GetSingleOrDefault(),
            x => UntransferredPayments(request, x).ById(paymentId).GetSingleOrDefault(),
            x => UnconfirmedRefundPayments(request, x).ById(paymentId).GetSingleOrDefault());

        var payment = unsettled ?? untransferred ?? unconfirmed;

        if (payment == null)
        {
            return ServiceResult.Failure("That payment is not waiting to be reconciled");
        }

        /* Run here rather than queued: one payment is a bounded number of provider calls, and a site
           admin pressing a button is owed the answer rather than a promise that something will happen. The
           bulk action stays queued, where hundreds of round trips cannot be waited on. */
        var result = await _paymentService.ResolvePaymentSettlement(payment.Id);

        if (!result.Success)
        {
            return result;
        }

        return ServiceResult.Successful(result.Transferred
            ? "Payment reconciled and the group's share sent"
            : "Payment reconciled");
    }

    public async Task<ServiceResult> ReconcilePayments(
        IMemberServiceRequest request, IReadOnlyCollection<Guid> paymentIds)
    {
        var payments = await GetPendingPayments(request, paymentIds);

        if (payments.Count == 0)
        {
            return ServiceResult.Failure("None of those payments are waiting to be reconciled");
        }

        /* Queued rather than run here, unlike the row action: a table's worth of payments is a table's
           worth of provider round trips, and a request cannot be held open for those. */
        foreach (var payment in payments)
        {
            _paymentService.EnqueueResolvePaymentSettlementJob(payment.Id);
        }

        return ServiceResult.Successful(
            ToOutcomeMessage("Queued", payments.Count, paymentIds.Count));
    }

    /* Looked up through the same scoping the page lists by, so a posted id can no more reach a payment on
       another platform or in another environment than the page could show one. How much is left to refund
       is the service's to rule on - it is the same question whether the refund is raised here or elsewhere. */
    public async Task<ServiceResult> RefundPayment(
        IMemberServiceRequest request, Guid paymentId, RefundPaymentModel model)
    {
        var payment = await GetSiteAdminRestrictedContent(
            request,
            x => x.PaymentRepository
                .Query()
                .ForEnvironment(request.Environment)
                .ForPlatform(request.Platform)
                .ById(paymentId)
                .WithDetails()
                .GetSingleOrDefault());

        if (payment == null)
        {
            return ServiceResult.Failure("Payment not found");
        }

        return await _paymentService.RefundPayment(request, payment, model);
    }

    public async Task<ServiceResult> UnignorePayment(
        IMemberServiceRequest request, Guid paymentId)
        => await SetPaymentIgnored(request, paymentId, ignored: false);

    /* Everything a site admin has told reconciliation to ignore. Not filtered on what it is missing: a
       payment is ignored because nothing will ever answer for it, which outlives whichever part was
       outstanding. */
    private static IPaymentQueryBuilder IgnoredPayments(
        IServiceRequest request, IUnitOfWork unitOfWork)
        => unitOfWork.PaymentRepository
            .Query()
            .ForEnvironment(request.Environment)
            .ForPlatform(request.Platform)
            .Paid()
            .IgnoredForReconciliation();

    /* What an ignored payment would do if it were reconciled after all. It has not been read, so which
       part is outstanding is read off the row rather than off which query found it. */
    private static ChapterPaymentItemViewModel ToChapterPaymentItem(
        PaymentMemberDto dto,
        IReadOnlyDictionary<Guid, PaymentRefund[]> refundsByPayment,
        IReadOnlyDictionary<Guid, PaymentTransfer> transfersByPayment)
    {
        var refunds = refundsByPayment.TryGetValue(dto.Payment.Id, out var found) ? found : [];

        /* Status is what says where a refund got to, and only Refunded means the money has gone -
           a failed one can carry a refunded date and still have returned the money to us. */
        var refunded = refunds
            .Where(x => x.Status == PaymentRefundStatusType.Refunded)
            .Sum(x => x.ActualAmount ?? x.Amount);

        var payment = dto.Payment;

        return new ChapterPaymentItemViewModel
        {
            ChapterAmount = transfersByPayment.TryGetValue(payment.Id, out var transfer)
                ? transfer.Amount
                : null,
            HasRefund = refunds.Length > 0,
            Member = dto.Member,
            Payment = payment,
            /* Gates the button that refunds through the provider, so it also asks whether there is a
               charge to refund against - a payment settled before charge ids were recorded names none. */
            RefundableAmount = payment.ExternalChargeId != null
                ? payment.RefundableAmount(refunds)
                : null,
            RefundedAmount = refunded > 0 ? refunded : null
        };
    }

    /* Read off the row rather than off which query found it, since an ignored payment has not been read.
       A settled one is listed as a transfer record even where what it is really waiting on is a refund
       outcome: telling those apart needs the refunds, and the kind here only has to say that un-ignoring
       it writes to our own records rather than moving money. */
    private static PaymentReconciliationType ToIgnoredType(
        Payment payment, IReadOnlySet<Guid> payableChapterIds)
        => payment.ActualAmount != null
            ? PaymentReconciliationType.TransferRecord
            : ToUnsettledType(payment, payableChapterIds);

    private static PaymentRefundItemViewModel ToRefundItem(
        PaymentRefund refund,
        Payment payment,
        IReadOnlyDictionary<Guid, string> chapterNames,
        IReadOnlyDictionary<Guid, decimal> outstandingByRefund,
        IReadOnlyDictionary<Guid, decimal> reversedByRefund)
        => new PaymentRefundItemViewModel
        {
            ChapterName = payment.ChapterId != null && chapterNames.TryGetValue(payment.ChapterId.Value, out var name)
                ? name
                : null,
            OutstandingAmount = outstandingByRefund.TryGetValue(refund.Id, out var outstanding)
                ? outstanding
                : null,
            Payment = payment,
            Refund = refund,
            ReversedAmount = reversedByRefund.TryGetValue(refund.Id, out var reversed)
                ? reversed
                : null
        };

    private static PaymentReconciliationItemViewModel ToItem(
        PaymentReconciliationDto dto,
        IReadOnlyDictionary<Guid, string> chapterNames,
        PaymentReconciliationType pending)
    {
        var payment = dto.Payment;

        return new PaymentReconciliationItemViewModel
        {
            ChapterName = payment.ChapterId != null && chapterNames.TryGetValue(payment.ChapterId.Value, out var name)
                ? name
                : null,
            FailureReason = dto.Reconciliation?.FailureReason,
            Payment = payment,
            Pending = pending
        };
    }

    /* Says so when fewer were acted on than were pressed. That gap means the page had gone stale - a
       payment reconciled or ignored since it was rendered - which is worth stating rather than quietly
       rounding down to a number that looks like success. */
    private static string ToOutcomeMessage(string verb, int acted, int requested)
        => acted == requested
            ? $"{verb} {acted} payment{(acted == 1 ? string.Empty : "s")}"
            : $"{verb} {acted} of {requested} payments; the rest are no longer outstanding";

    /* What reading an unsettled payment's settlement will go on to do. A connected account to pay is what
       decides it, not the payment belonging to a group - the same test TransferConnectedAccountShare
       makes.

       A legacy destination charge is the one this can overstate: the provider already transferred it, and
       that is only knowable once the settlement has been read. Overstating is the safe direction - the
       page warns of a payment that turns out to move nothing, rather than moving money unannounced. */
    private static PaymentReconciliationType ToUnsettledType(
        Payment payment, IReadOnlySet<Guid> payableChapterIds)
        => payment.ChapterId != null && payableChapterIds.Contains(payment.ChapterId.Value)
            ? PaymentReconciliationType.SettlementAndTransfer
            : PaymentReconciliationType.Settlement;

    /* One definition of what is pending, shared by the page and the actions on it - a table listing
       something a button would not touch, or missing something it would, is worse than no table.

       Scoped to one platform, like the rest of the site admin area: a payment carries the platform of the
       group it was taken for, so the platform's own site is where its payments are reconciled.

       Only paid payments: an abandoned checkout has nothing at the provider to read, so including them
       would queue a job per dead session that can only fail its retries. */
    private static IPaymentQueryBuilder UnsettledPayments(
        IServiceRequest request, IUnitOfWork unitOfWork)
        => unitOfWork.PaymentRepository
            .Query()
            .ForEnvironment(request.Environment)
            .ForPlatform(request.Platform)
            .Paid()
            .NotIgnoredForReconciliation()
            .WithoutSettlement();

    /// <inheritdoc cref="UnsettledPayments"/>
    private static IPaymentQueryBuilder UnconfirmedRefundPayments(
        IServiceRequest request, IUnitOfWork unitOfWork)
        => unitOfWork.PaymentRepository
            .Query()
            .ForEnvironment(request.Environment)
            .ForPlatform(request.Platform)
            .Paid()
            .NotIgnoredForReconciliation()
            .WithUnconfirmedRefund();

    /// <inheritdoc cref="UnsettledPayments"/>
    private static IPaymentQueryBuilder UntransferredPayments(
        IServiceRequest request, IUnitOfWork unitOfWork)
        => unitOfWork.PaymentRepository
            .Query()
            .ForEnvironment(request.Environment)
            .ForPlatform(request.Platform)
            .Paid()
            .NotIgnoredForReconciliation()
            .WithUnrecordedTransfer();

    /* The groups the pending payments name, and which of them have an account to be paid through, asked
       for once the payments are known. A second round trip, rather than every group in one: nothing here
       wants a group no pending payment names, and a site with nothing pending asks for none at all.

       The accounts are read exactly as ResolvePaymentSettlement reads them, unfiltered by environment, so
       what this page predicts is what that job will do rather than something close to it. */
    private async Task<(IReadOnlyDictionary<Guid, string> Names, IReadOnlySet<Guid> Payable)> GetChapters(
        PlatformType platform, IEnumerable<Payment> payments)
    {
        var chapterIds = payments
            .Where(x => x.ChapterId != null)
            .Select(x => x.ChapterId!.Value)
            .Distinct()
            .ToArray();

        if (chapterIds.Length == 0)
        {
            return (new Dictionary<Guid, string>(), new HashSet<Guid>());
        }

        var (chapters, paymentAccounts) = await _unitOfWork.Run(
            x => x.ChapterRepository.GetByIds(platform, chapterIds),
            x => x.ChapterPaymentAccountRepository
                .Query()
                .ForChapters(chapterIds)
                .GetAll());

        return (
            chapters.ToDictionary(x => x.Id, x => x.Name),
            paymentAccounts.Select(x => x.ChapterId).ToHashSet());
    }

    /* The payments among the ids posted that are still outstanding: the intersection of what the site
       admin was looking
       at and what is still pending. A stale id - reconciled or ignored since the page was rendered, or
       never on this platform at all - is simply not found, so a posted form can no more reach past the page
       than a row action can. */
    private async Task<IReadOnlyCollection<Payment>> GetPendingPayments(
        IMemberServiceRequest request, IReadOnlyCollection<Guid> paymentIds)
    {
        if (paymentIds.Count == 0)
        {
            return [];
        }

        var (unsettled, untransferred, unconfirmed) = await GetSiteAdminRestrictedContent(
            request,
            x => UnsettledPayments(request, x).ByIds(paymentIds).GetAll(),
            x => UntransferredPayments(request, x).ByIds(paymentIds).GetAll(),
            x => UnconfirmedRefundPayments(request, x).ByIds(paymentIds).GetAll());

        /* Distinct by id rather than concatenated: the first two are disjoint by construction - a payment
           has either no settlement or a settled one - but an unconfirmed refund says nothing about either,
           so the same payment can be waiting on a refund and on its transfer id at once. */
        return [.. unsettled
            .Concat(untransferred)
            .Concat(unconfirmed)
            .DistinctBy(x => x.Id)];
    }

    /* Un-ignoring a payment nothing has ever recorded anything about writes a row saying so, rather than
       nothing: the row is the record of the decision, and a reader cannot tell a payment never ignored from
       one ignored and released if only the first leaves a trace. */
    private void SetIgnored(Payment payment, PaymentReconciliation? reconciliation, bool ignored)
    {
        if (reconciliation == null)
        {
            _unitOfWork.PaymentReconciliationRepository.Add(new PaymentReconciliation
            {
                IgnoredUtc = ignored ? DateTime.UtcNow : null,
                PaymentId = payment.Id
            });

            return;
        }

        reconciliation.IgnoredUtc = ignored ? DateTime.UtcNow : null;
        _unitOfWork.PaymentReconciliationRepository.Update(reconciliation);
    }

    /* Both directions through one method, because they are one decision written twice: an instruction to
       ignore that could not be undone the same way it was made would be a trap. */
    private async Task<ServiceResult> SetPaymentIgnored(
        IMemberServiceRequest request, Guid paymentId, bool ignored)
    {
        var payment = await GetSiteAdminRestrictedContent(
            request,
            x => x.PaymentRepository
                .Query()
                .ForEnvironment(request.Environment)
                .ForPlatform(request.Platform)
                .ById(paymentId)
                .GetSingleOrDefault());

        if (payment == null)
        {
            return ServiceResult.Failure("Payment not found");
        }

        var reconciliation = await _unitOfWork.PaymentReconciliationRepository
            .Query()
            .ForPayment(payment.Id)
            .GetSingleOrDefault()
            .Run();

        SetIgnored(payment, reconciliation, ignored);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful(
            ignored ? "Payment ignored" : "Payment will be reconciled again");
    }
}