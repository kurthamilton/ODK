using ODK.Core.Payments;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IPaymentRepository : IReadWriteRepository<Payment>
{
    IDeferredQueryMultiple<PaymentDto> GetDtosByMemberId(Guid memberId);
}