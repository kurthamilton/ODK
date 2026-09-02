using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IPaymentRepository : IReadWriteRepository<Payment, IPaymentQueryBuilder>
{
}