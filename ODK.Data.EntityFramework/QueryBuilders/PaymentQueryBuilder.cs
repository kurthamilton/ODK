using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class PaymentQueryBuilder : DatabaseEntityQueryBuilder<Payment, IPaymentQueryBuilder>, IPaymentQueryBuilder
{
    public PaymentQueryBuilder(DbContext context)
        : base(context, BaseQuery(context))
    {
    }

    protected override IPaymentQueryBuilder Builder => this;

    public IPaymentQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IPaymentQueryBuilder ForEnvironment(EnvironmentType environment)
    {
        Query = Query.Where(x => x.Environment == environment);
        return this;
    }

    public IPaymentQueryBuilder ForExternalReference(string reference)
    {
        Query = Query.Where(x =>
            x.ExternalChargeId == reference ||
            x.ExternalId == reference ||
            x.Reference == reference);
        return this;
    }

    public IPaymentQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IPaymentQueryBuilder ForPlatform(PlatformType platform)
    {
        Query = Query.Where(x => x.Platform == platform);
        return this;
    }

    public IPaymentQueryBuilder ForSite()
    {
        Query = Query.Where(x => x.ChapterId == null);
        return this;
    }

    public IPaymentQueryBuilder IgnoredForReconciliation()
    {
        var ignored = IgnoredReconciliations();
        Query = Query.Where(x => ignored.Any(r => r.PaymentId == x.Id));
        return this;
    }

    /* Not ignored covers the payment that has never been reconciled at all, which has no reconciliation
       row to read a date off - so this is the absence of an ignoring one rather than a null date. */
    public IPaymentQueryBuilder NotIgnoredForReconciliation()
    {
        var ignored = IgnoredReconciliations();
        Query = Query.Where(x => !ignored.Any(r => r.PaymentId == x.Id));
        return this;
    }

    public IPaymentQueryBuilder Paid()
    {
        Query = Query.Where(x => x.PaidUtc != null);
        return this;
    }

    public IPaymentQueryBuilder WithUnrecordedTransfer()
    {
        /* Withholding nothing is what separates a payment transferred before ids were recorded from one
           whose share was kept back against a debt: the second made no transfer, so there is none to find
           and nothing for the backfill to do. */
        var unrecorded = Set<PaymentTransfer>()
            .Where(x =>
                x.CompletedUtc != null &&
                x.ExternalId == null &&
                x.WithheldAmount == null);

        Query = Query.Where(x => unrecorded.Any(t => t.PaymentId == x.Id));
        return this;
    }

    // Mirrors IPaymentRefundQueryBuilder.Unconfirmed, asked of the payments carrying such a refund.
    public IPaymentQueryBuilder WithUnconfirmedRefund()
    {
        var unconfirmed = Set<PaymentRefund>()
            .Where(x => x.Status == PaymentRefundStatusType.Pending);

        Query = Query.Where(x => unconfirmed.Any(r => r.PaymentId == x.Id));
        return this;
    }

    public IPaymentQueryBuilder WithoutSettlement()
    {
        Query = Query.Where(x => x.ActualAmount == null);
        return this;
    }

    public IQueryBuilder<PaymentChapterDto> WithChapter()
    {
        var query =
            from payment in Query
            from chapter in Set<Chapter>()
                .Where(x => x.Id == payment.ChapterId)
            select new PaymentChapterDto
            {
                Chapter = chapter,
                Payment = payment
            };

        return ProjectTo(query);
    }

    public IQueryBuilder<PaymentMemberDto> WithMember()
    {
        var query =
            from payment in Query
            from member in Set<Member>()
                .Where(x => x.Id == payment.MemberId)
            select new PaymentMemberDto
            {
                Member = member,
                Payment = payment
            };

        return ProjectTo(query);
    }

    public IQueryBuilder<PaymentDetailsDto> WithDetails()
    {
        var query =
            from payment in Query
            from reconciliation in Set<PaymentReconciliation>()
                .Where(x => x.PaymentId == payment.Id)
                .DefaultIfEmpty()
            from transfer in Set<PaymentTransfer>()
                .Where(x => x.PaymentId == payment.Id)
                .DefaultIfEmpty()
            select new PaymentDetailsDto
            {
                Payment = payment,
                Reconciliation = reconciliation,
                Refunds = Set<PaymentRefund>()
                    .Where(x => x.PaymentId == payment.Id)
                    .ToArray(),
                /* Reached through the transfer rather than through the joined one above, which is null for
                   a payment that has none - and a null cannot be asked for its id. */
                Reversals = Set<PaymentTransferReversal>()
                    .Where(x => Set<PaymentTransfer>()
                        .Any(t => t.Id == x.PaymentTransferId && t.PaymentId == payment.Id))
                    .ToArray(),
                Transfer = transfer
            };

        return ProjectTo(query);
    }

    public IQueryBuilder<PaymentReconciliationDto> WithReconciliation()
    {
        var query =
            from payment in Query
            from reconciliation in Set<PaymentReconciliation>()
                .Where(x => x.PaymentId == payment.Id)
                .DefaultIfEmpty()
            select new PaymentReconciliationDto
            {
                Payment = payment,
                Reconciliation = reconciliation
            };

        return ProjectTo(query);
    }

    private static IQueryable<Payment> BaseQuery(DbContext context)
    {
        // exclude payments for an expired checkout session by default
        return
            from payment in context.Set<Payment>()
                .Include(x => x.Currency)
            from paymentCheckoutSession in context.Set<PaymentCheckoutSession>()
                .Where(x => x.PaymentId == payment.Id)
                .DefaultIfEmpty()
            where paymentCheckoutSession == null || paymentCheckoutSession.ExpiredUtc == null
            select payment;
    }

    private IQueryable<PaymentReconciliation> IgnoredReconciliations()
        => Set<PaymentReconciliation>().Where(x => x.IgnoredUtc != null);
}