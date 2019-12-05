using System;
using System.Threading.Tasks;
using ODK.Core.Payments;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class PaymentRepository : RepositoryBase, IPaymentRepository
    {
        public PaymentRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<Guid> CreatePayment(Payment payment)
        {
            return await Context
                .Insert(payment)
                .GetIdentityAsync();
        }
    }
}
