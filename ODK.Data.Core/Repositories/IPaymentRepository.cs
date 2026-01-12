using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Payments;

namespace ODK.Data.Core.Repositories;

public interface IPaymentRepository : IReadWriteRepository<Payment>
{
    IDeferredQueryMultiple<Payment> GetAll();

    IDeferredQueryMultiple<PaymentChapterDto> GetChapterDtosByMemberId(Guid memberId);

    IDeferredQueryMultiple<PaymentDto> GetMemberChapterPayments(Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosByChapterId(Guid chapterId);

    IDeferredQueryMultiple<PaymentDto> GetSitePaymentsByMemberId(Guid memberId);
}