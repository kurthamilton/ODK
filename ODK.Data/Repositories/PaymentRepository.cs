using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    }
}
