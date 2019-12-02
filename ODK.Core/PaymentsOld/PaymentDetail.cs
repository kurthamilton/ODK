using System;

namespace ODK.Core.Payments
{
    [Obsolete("TODO: replace")]
    public class PaymentDetail
    {
        public PaymentDetail(double amount, int nodeId, Guid paymentId)
        {
            Amount = amount;
            NodeId = nodeId;
            PaymentId = paymentId;
        }

        public double Amount { get; }

        public int NodeId { get; }

        public Guid PaymentId { get; }
    }
}
