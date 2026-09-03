using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentReconciliationRepository
    : ReadWriteRepositoryBase<PaymentReconciliation, IPaymentReconciliationQueryBuilder>,
    IPaymentReconciliationRepository
{
    public PaymentReconciliationRepository(DbContext context)
        : base(context)
    {
    }

    public override IPaymentReconciliationQueryBuilder Query()
        => CreateQueryBuilder(context => new PaymentReconciliationQueryBuilder(context));
}
