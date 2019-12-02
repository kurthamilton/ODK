using System;
using System.Linq;
using ODK.Core.Payments;
using ODK.Data.Repositories;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Payments
{
    public class PaymentService
    {
        private readonly PaymentRepository _paymentsDataService;

        public PaymentService(PaymentRepository paymentsDataService)
        {
            _paymentsDataService = paymentsDataService;
        }

        public void CreatePayment(Guid? id, OdkMember currentMember, string currencyCode, int nodeId, double amount,
            bool complete = false)
        {
            id = id ?? Guid.NewGuid();

            PaymentDetail paymentDetail = new PaymentDetail(amount, nodeId, id.Value);
            
            Payment payment = new Payment(id.Value, id.Value.ToString(), currentMember?.Id ?? 0, currencyCode, DateTime.Now, new[] { paymentDetail });
            _paymentsDataService.CreatePayment(payment);

            if (complete)
            {
                _paymentsDataService.CompletePayment(payment.Id);                
            }
        }

        public ServiceResult CompletePayment(Guid id, OdkMember currentMember, int nodeId)
        {
            Payment payment = _paymentsDataService.GetIncompletePayment(id.ToString());
            if (payment == null || payment.MemberId != currentMember.Id || !payment.Details.Any(x => x.NodeId == nodeId))
            {
                return new ServiceResult(false);
            }

            _paymentsDataService.CompletePayment(id);
            return new ServiceResult(true);
        }
    }
}
