using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentTransferReversalRepository
    : ReadWriteRepositoryBase<PaymentTransferReversal, IPaymentTransferReversalQueryBuilder>,
    IPaymentTransferReversalRepository
{
    public PaymentTransferReversalRepository(DbContext context)
        : base(context)
    {
    }

    public override IPaymentTransferReversalQueryBuilder Query()
        => CreateQueryBuilder(context => new PaymentTransferReversalQueryBuilder(context));
}
