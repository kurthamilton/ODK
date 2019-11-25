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

        public void CompletePayment(Guid id)
        {
            throw new NotImplementedException();
        }

        public void CreatePayment(Payment payment)
        {
            throw new NotImplementedException();
        }

        public Payment GetIncompletePayment(string identifier)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Payment> GetCompletePayments(int memberId)
        {
            throw new NotImplementedException();
        }

        private static Payment ReadPayment(SqlDataReader reader, IReadOnlyCollection<PaymentDetail> paymentDetails)
        {
            throw new NotImplementedException();
        }
    }
}
