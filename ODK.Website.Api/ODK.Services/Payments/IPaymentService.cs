using System;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Payments
{
    public interface IPaymentService
    {
        Task<bool> MakePayment(Member member, double amount, string cardToken, string reference);
    }
}
