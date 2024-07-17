using ODK.Core.Payments;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentRepository : ReadWriteRepositoryBase<Payment>, IPaymentRepository
{
    public PaymentRepository(OdkContext context)
        : base(context)
    {
    }
}
