using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentTransferRepository
    : ReadWriteRepositoryBase<PaymentTransfer, IPaymentTransferQueryBuilder>, IPaymentTransferRepository
{
    public PaymentTransferRepository(DbContext context)
        : base(context)
    {
    }

    public override IPaymentTransferQueryBuilder Query()
        => CreateQueryBuilder(context => new PaymentTransferQueryBuilder(context));
}
