using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Payments;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentCheckoutSessionRepository : ReadWriteRepositoryBase<PaymentCheckoutSession>, IPaymentCheckoutSessionRepository
{
    public PaymentCheckoutSessionRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<PaymentCheckoutSessionDto> GetDtoByMemberId(Guid memberId, string sessionId)
    {
        var query =
            from session in Set()
            from payment in Set<Payment>()
                .Where(x => x.Id == session.PaymentId)
            where session.MemberId == memberId && session.SessionId == sessionId
            select new PaymentCheckoutSessionDto
            {
                Payment = payment,
                Session = session
            };

        return query.DeferredSingle();
    }
}