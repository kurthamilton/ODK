using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentRepository : ReadWriteRepositoryBase<Payment>, IPaymentRepository
{
    public PaymentRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<PaymentDto> GetDtosByMemberId(Guid memberId)
    {
        var query =
            from payment in Set()
            from chapter in Set<Chapter>()
            where chapter.Id == payment.ChapterId
            select new PaymentDto
            {
                Chapter = chapter,
                Payment = payment
            };

        return query.DeferredMultiple();
    }

    protected override IQueryable<Payment> Set() => base.Set()
        .Include(x => x.Currency);
}
