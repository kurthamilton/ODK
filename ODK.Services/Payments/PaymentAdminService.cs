using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
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
        var (unsettled, untransferred, ignored) = await GetSiteAdminRestrictedContent(
            request,
            x => UnsettledPayments(request, x).GetAll(),
            x => UntransferredPayments(request, x).GetAll(),
            x => IgnoredPayments(request, x).GetAll());

        var (chapterNames, payableChapterIds) = await GetChapters(
            request.Platform, unsettled.Concat(untransferred).Concat(ignored));

        var items = unsettled
            .Select(x => ToItem(x, chapterNames, ToUnsettledType(x, payableChapterIds)))
            .Concat(untransferred
                .Select(x => ToItem(x, chapterNames, PaymentReconciliationType.TransferRecord)));

        return new PaymentReconciliationViewModel
        {
            /* An ignored payment keeps the kind it would have been, so the page can still say what
               un-ignoring it would do. */
            Ignored = ignored
                .Select(x => ToItem(x, chapterNames, ToIgnoredType(x, payableChapterIds)))
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray(),
            Payments = items
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray()
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

        return new PaymentRefundsViewModel
        {
            Refunds = refunds
                .Select(x => ToRefundItem(x, paymentsById[x.PaymentId], chapterNames))
                .OrderByDescending(x => x.Refund.RequestedUtc)
                .ToArray()
        };
    }

    public async Task<ChapterPaymentsViewModel> GetPayments(
        IMemberChapterAdminServiceRequest request)
    {
        var (environment, chapter) = (request.Environment, request.Chapter);

        var (payments, paymentAccount) = await GetChapterAdminRestrictedContent(
            request,
            x => x.PaymentRepository.GetMemberDtosByChapterId(chapter.Id),
            x => x.ChapterPaymentAccountRepository
                .Query()
                .ForChapter(chapter.Id)
                .ForEnvironment(environment)
                .GetSingleOrDefault());

        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            PaymentAccountEnabled = paymentAccount?.SetupComplete() == true,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray()
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

        foreach (var payment in payments)
        {
            payment.ReconciliationIgnoredUtc = DateTime.UtcNow;
            _unitOfWork.PaymentRepository.Update(payment);
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
        var (unsettled, untransferred) = await GetSiteAdminRestrictedContent(
            request,
            x => UnsettledPayments(request, x).ById(paymentId).GetSingleOrDefault(),
            x => UntransferredPayments(request, x).ById(paymentId).GetSingleOrDefault());

        var payment = unsettled ?? untransferred;

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

    public async Task<ServiceResult> RecordPaymentRefund(
        IMemberServiceRequest request, RecordPaymentRefundModel model)
    {
        if (model.Amount <= 0)
        {
            return ServiceResult.Failure("Enter the amount that was refunded");
        }

        var reference = model.PaymentReference.Trim();

        var matches = await GetSiteAdminRestrictedContent(
            request,
            x => x.PaymentRepository
                .Query()
                .ForEnvironment(request.Environment)
                .ForPlatform(request.Platform)
                .Paid()
                .ForExternalReference(reference)
                .GetAll());

        if (matches.Count == 0)
        {
            return ServiceResult.Failure($"No payment found for '{reference}'");
        }

        if (matches.Count > 1)
        {
            /* Our own reference is shared by every payment for a subscription, so it cannot pick one out.
               The provider's charge names exactly one. */
            return ServiceResult.Failure(
                $"'{reference}' matches {matches.Count} payments - use the provider's charge id");
        }

        var payment = matches.Single();

        var existing = await _unitOfWork.PaymentRefundRepository
            .Query()
            .ForPayment(payment.Id)
            .Live()
            .GetAll()
            .Run();

        var validation = ValidateRefund(payment, existing, model);

        if (validation != null)
        {
            return ServiceResult.Failure(validation);
        }

        /* The group covers what the refund cost us, so a fee the provider gave back is one less thing it
           cost and comes off what the group owes. Null for a site payment: there is no group to owe it. */
        var chapterAmount = payment.ChapterId != null
            ? model.Amount - model.FeeReturnedAmount
            : (decimal?)null;

        var utcNow = DateTime.UtcNow;

        var refund = _unitOfWork.PaymentRefundRepository.Add(new PaymentRefund
        {
            ActualAmount = model.Amount,
            Amount = model.Amount,
            ChapterAmount = chapterAmount,
            ExternalId = model.ExternalId,
            ExternalReversalId = model.ExternalReversalId,
            FeeReturnedAmount = model.FeeReturnedAmount > 0 ? model.FeeReturnedAmount : null,
            PaymentId = payment.Id,
            Reason = model.Reason,
            RefundedUtc = utcNow,
            RequestedByMemberId = request.CurrentMember.Id,
            RequestedUtc = utcNow,
            ResolvedByMemberId = request.CurrentMember.Id,
            ResolvedUtc = utcNow,
            ReversedAmount = model.ReversedAmount > 0 ? model.ReversedAmount : null,
            ReversedUtc = model.ReversedAmount > 0 ? utcNow : null,
            SettlementCurrencyCode = model.FeeReturnedAmount > 0 ? payment.SettlementCurrencyCode : null,
            Status = PaymentRefundStatusType.Refunded
        });

        var outstanding = (chapterAmount ?? 0) - model.ReversedAmount;

        if (payment.ChapterId != null && outstanding > 0)
        {
            /* What the reversal could not take back. Recorded rather than absorbed, so the sum of a group's
               adjustments is the answer to what it owes - even while nothing collects them yet. */
            _unitOfWork.ChapterPaymentAdjustmentRepository.Add(new ChapterPaymentAdjustment
            {
                Amount = -outstanding,
                ChapterId = payment.ChapterId.Value,
                CreatedUtc = utcNow,
                CurrencyId = payment.CurrencyId,
                Description = $"Refund of payment {payment.Reference}",
                PaymentRefundId = refund.Id,
                RecoveredAmount = 0,
                Type = ChapterPaymentAdjustmentType.RefundShortfall
            });
        }

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful(outstanding > 0
            ? $"Refund recorded; the group owes {outstanding:0.00}"
            : "Refund recorded");
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
    private static PaymentReconciliationType ToIgnoredType(
        Payment payment, IReadOnlySet<Guid> payableChapterIds)
        => payment.ActualAmount != null
            ? PaymentReconciliationType.TransferRecord
            : ToUnsettledType(payment, payableChapterIds);

    private static PaymentRefundItemViewModel ToRefundItem(
        PaymentRefund refund,
        Payment payment,
        IReadOnlyDictionary<Guid, string> chapterNames)
        => new PaymentRefundItemViewModel
        {
            ChapterName = payment.ChapterId != null && chapterNames.TryGetValue(payment.ChapterId.Value, out var name)
                ? name
                : null,
            OutstandingAmount = refund.ChapterAmount != null
                ? refund.ChapterAmount - (refund.ReversedAmount ?? 0)
                : null,
            Payment = payment,
            Refund = refund
        };

    private static PaymentReconciliationItemViewModel ToItem(
        Payment payment,
        IReadOnlyDictionary<Guid, string> chapterNames,
        PaymentReconciliationType pending)
        => new PaymentReconciliationItemViewModel
        {
            ChapterName = payment.ChapterId != null && chapterNames.TryGetValue(payment.ChapterId.Value, out var name)
                ? name
                : null,
            Payment = payment,
            Pending = pending
        };

    /* Says so when fewer were acted on than were pressed. That gap means the page had gone stale - a
       payment reconciled or ignored since it was rendered - which is worth stating rather than quietly
       rounding down to a number that looks like success. */
    private static string ToOutcomeMessage(string verb, int acted, int requested)
        => acted == requested
            ? $"{verb} {acted} payment{(acted == 1 ? string.Empty : "s")}"
            : $"{verb} {acted} of {requested} payments; the rest are no longer outstanding";

    /* What a refund cannot exceed. The settlement is what says what the charge and the group's share
       actually were, so a payment that has never been read cannot have either checked - and a reversal
       claimed against one would be a number nothing can contradict. */
    private static string? ValidateRefund(
        Payment payment, IReadOnlyCollection<PaymentRefund> existing, RecordPaymentRefundModel model)
    {
        if (payment.ActualAmount == null)
        {
            return model.ReversedAmount > 0
                ? "Reconcile this payment before recording a reversal against it"
                : null;
        }

        var refundedAlready = existing.Sum(x => x.ActualAmount ?? x.Amount);

        if (refundedAlready + model.Amount > payment.ActualAmount)
        {
            return
                $"That would refund more than the payment: {payment.ActualAmount} taken, " +
                $"{refundedAlready} already refunded";
        }

        var reversedAlready = existing.Sum(x => x.ReversedAmount ?? 0);
        var share = payment.ActualConnectedAccountAmount ?? 0;

        if (reversedAlready + model.ReversedAmount > share)
        {
            return
                $"That would reverse more than the group was sent: {share} transferred, " +
                $"{reversedAlready} already reversed";
        }

        return null;
    }

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

        var (unsettled, untransferred) = await GetSiteAdminRestrictedContent(
            request,
            x => UnsettledPayments(request, x).ByIds(paymentIds).GetAll(),
            x => UntransferredPayments(request, x).ByIds(paymentIds).GetAll());

        // Disjoint by construction - a payment has either no settlement or a settled one.
        return [.. unsettled, .. untransferred];
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

        payment.ReconciliationIgnoredUtc = ignored ? DateTime.UtcNow : null;

        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful(
            ignored ? "Payment ignored" : "Payment will be reconciled again");
    }
}
