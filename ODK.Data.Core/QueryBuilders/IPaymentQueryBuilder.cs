using ODK.Core.Payments;
using ODK.Data.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentQueryBuilder : IDatabaseEntityQueryBuilder<Payment, IPaymentQueryBuilder>
{
    IPaymentQueryBuilder ForChapter(Guid chapterId);

    IPaymentQueryBuilder ForMember(Guid memberId);

    IPaymentQueryBuilder ForSite();

    IQueryBuilder<PaymentChapterDto> WithChapter();

    IQueryBuilder<PaymentMemberDto> WithMember();
}
