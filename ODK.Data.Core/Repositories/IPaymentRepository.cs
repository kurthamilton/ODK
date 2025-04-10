using ODK.Core.Payments;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IPaymentRepository : IReadWriteRepository<Payment>
{
    IDeferredQueryMultiple<PaymentChapterDto> GetChapterDtosByMemberId(Guid memberId);

    IDeferredQueryMultiple<PaymentDto> GetMemberChapterPayments(Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosByChapterId(Guid chapterId);

    IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosPendingReconciliation(Guid chapterId);    
}