using ODK.Core.Payments;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IPaymentCheckoutSessionRepository : IReadWriteRepository<PaymentCheckoutSession>
{
    IDeferredQuerySingleOrDefault<PaymentCheckoutSession> GetByMemberId(Guid memberId, string sessionId);
}
