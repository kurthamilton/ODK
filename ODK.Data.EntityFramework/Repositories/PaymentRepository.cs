using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentRepository : ReadWriteRepositoryBase<Payment, IPaymentQueryBuilder>, IPaymentRepository
{
    public PaymentRepository(DbContext context)
        : base(context)
    {
    }

    public override IPaymentQueryBuilder Query() => CreateQueryBuilder(context => new PaymentQueryBuilder(context));
}