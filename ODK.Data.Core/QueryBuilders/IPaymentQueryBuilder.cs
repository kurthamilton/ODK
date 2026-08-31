using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentQueryBuilder : IDatabaseEntityQueryBuilder<Payment, IPaymentQueryBuilder>
{
    IPaymentQueryBuilder ForChapter(Guid chapterId);

    IPaymentQueryBuilder ForEnvironment(EnvironmentType environment);

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
    /// Payments whose settlement has never been read back from the payment provider.
    /// </summary>
    IPaymentQueryBuilder WithoutSettlement();

    IQueryBuilder<PaymentChapterDto> WithChapter();

    IQueryBuilder<PaymentMemberDto> WithMember();
}
