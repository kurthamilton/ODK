using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IPaymentRefundRepository : IReadWriteRepository<PaymentRefund, IPaymentRefundQueryBuilder>
{
}
