using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentRefundRepository
    : ReadWriteRepositoryBase<PaymentRefund, IPaymentRefundQueryBuilder>, IPaymentRefundRepository
{
    public PaymentRefundRepository(DbContext context)
        : base(context)
    {
    }

    public override IPaymentRefundQueryBuilder Query()
        => CreateQueryBuilder(context => new PaymentRefundQueryBuilder(context));
}
