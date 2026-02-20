using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IPaymentRepository : IReadWriteRepository<Payment, IPaymentQueryBuilder>
{
    IDeferredQueryMultiple<PaymentChapterDto> GetChapterDtosByMemberId(Guid memberId);

    IDeferredQueryMultiple<Payment> GetMemberChapterPayments(Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosByChapterId(Guid chapterId);

    IDeferredQueryMultiple<Payment> GetSitePaymentsByMemberId(Guid memberId);
}