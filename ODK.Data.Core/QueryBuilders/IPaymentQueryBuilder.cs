using ODK.Core.Payments;
using ODK.Data.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentQueryBuilder : IDatabaseEntityQueryBuilder<Payment, IPaymentQueryBuilder>
{
    IPaymentQueryBuilder ForChapter(Guid chapterId);

    IPaymentQueryBuilder ForMember(Guid memberId);

    IPaymentQueryBuilder ForSite();

    /// <summary>
    /// Payments the member has paid for.
    /// </summary>
    IPaymentQueryBuilder Paid();

    /// <summary>
    /// Payments whose settlement has never been read back from the payment provider.
    /// </summary>
    IPaymentQueryBuilder WithoutSettlement();

    IQueryBuilder<PaymentChapterDto> WithChapter();

    IQueryBuilder<PaymentMemberDto> WithMember();
}
