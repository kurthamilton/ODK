using ODK.Core.Payments;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IPaymentReconciliationRepository : IReadWriteRepository<PaymentReconciliation>
{
    IDeferredQueryMultiple<PaymentReconciliation> GetByChapterId(Guid chapterId);
}
