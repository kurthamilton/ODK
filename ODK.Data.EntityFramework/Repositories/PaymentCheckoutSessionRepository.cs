using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentCheckoutSessionRepository : ReadWriteRepositoryBase<PaymentCheckoutSession>, IPaymentCheckoutSessionRepository
{
    public PaymentCheckoutSessionRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<PaymentCheckoutSession> GetByMemberId(Guid memberId, string sessionId)
        => Set()
            .Where(x => x.MemberId == memberId && x.SessionId == sessionId)
            .DeferredSingleOrDefault();
}