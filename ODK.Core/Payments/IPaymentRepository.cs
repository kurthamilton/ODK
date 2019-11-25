using System;
using System.Collections.Generic;

namespace ODK.Core.Payments
{
    public interface IPaymentRepository
    {
        void CompletePayment(Guid id);
        void CreatePayment(Payment payment);
        IReadOnlyCollection<Payment> GetCompletePayments(int memberId);
        Payment GetIncompletePayment(string identifier);
    }
}