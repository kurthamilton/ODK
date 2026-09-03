using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentQueryBuilder : IDatabaseEntityQueryBuilder<Payment, IPaymentQueryBuilder>
{
    IPaymentQueryBuilder ForChapter(Guid chapterId);

    IPaymentQueryBuilder ForEnvironment(EnvironmentType environment);

    /// <summary>
    /// Payments matching a reference as a site admin would have one to hand: the provider's charge, the
    /// provider's payment or subscription, or the reference recorded when the payment was taken.
    /// </summary>
    /// <remarks>
    /// Our own reference is not unique - a subscription's is the same on every payment for it - so a caller
    /// resolving a single payment has to cope with more than one match rather than assuming one.
    /// </remarks>
    IPaymentQueryBuilder ForExternalReference(string reference);

    IPaymentQueryBuilder ForMember(Guid memberId);

    /// <summary>
    /// Payments taken for <paramref name="platform"/>, which is the platform of the group the payment was
    /// for and not the site the member paid through - a Drunken Knitwits group's payment is a Drunken
    /// Knitwits payment wherever it was made. An exact match, so
    /// <see cref="PlatformType.Default"/> means that platform's own payments and not every payment.
    /// </summary>
    IPaymentQueryBuilder ForPlatform(PlatformType platform);

    IPaymentQueryBuilder ForSite();

    /// <summary>
    /// Payments a site admin has told reconciliation to ignore.
    /// </summary>
    IPaymentQueryBuilder IgnoredForReconciliation();

    /// <summary>
    /// Payments a site admin has not told reconciliation to ignore.
    /// </summary>
    IPaymentQueryBuilder NotIgnoredForReconciliation();

    /// <summary>
    /// Payments the member has paid for.
    /// </summary>
    IPaymentQueryBuilder Paid();

    /// <summary>
    /// Payments whose share reached the group before the transfer that moved it was recorded, so nothing
    /// names the transfer a refund would reverse.
    /// </summary>
    IPaymentQueryBuilder WithUnrecordedTransfer();

    /// <summary>
    /// Payments carrying a refund the provider took and has not said the outcome of, so what became of the
    /// member's money is unknown to us.
    /// </summary>
    IPaymentQueryBuilder WithUnconfirmedRefund();

    /// <summary>
    /// Payments whose settlement has never been read back from the payment provider.
    /// </summary>
    IPaymentQueryBuilder WithoutSettlement();

    IQueryBuilder<PaymentChapterDto> WithChapter();

    /// <summary>
    /// Each payment with everything recorded against it, in one read. For a caller that needs the whole
    /// picture of a payment - what a refund can be checked against, and what there is to reverse - rather
    /// than one that needs a column or two of it.
    /// </summary>
    IQueryBuilder<PaymentDetailsDto> WithDetails();

    IQueryBuilder<PaymentMemberDto> WithMember();

    /// <summary>
    /// Each payment with what the reconciliation job has to say about it, where it has anything to say -
    /// a payment no reconcile has stumbled on carries none.
    /// </summary>
    IQueryBuilder<PaymentReconciliationDto> WithReconciliation();
}
