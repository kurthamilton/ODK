using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Payments;

namespace ODK.Data.Core.Repositories;

public interface IPaymentCheckoutSessionRepository : IReadWriteRepository<PaymentCheckoutSession>
{
    IDeferredQuerySingle<PaymentCheckoutSessionDto> GetDtoByMemberId(Guid memberId, string sessionId);
}
