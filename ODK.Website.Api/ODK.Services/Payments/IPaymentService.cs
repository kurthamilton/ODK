using System;
using System.Threading.Tasks;

namespace ODK.Services.Payments
{
    public interface IPaymentService
    {
        Task<string> CreatePayment(Guid memberId, Guid subscriptionId, string successUrl, string cancelUrl);
    }
}
