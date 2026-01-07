using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentReconciliationRepository : ReadWriteRepositoryBase<PaymentReconciliation>,
    IPaymentReconciliationRepository
{
    public PaymentReconciliationRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<PaymentReconciliation> GetByChapterId(Guid chapterId)
        => Set<PaymentReconciliation>()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();
}
