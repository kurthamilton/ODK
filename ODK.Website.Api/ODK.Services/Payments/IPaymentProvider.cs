using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Payments
{
    public interface IPaymentProvider
    {
        bool HasExternalGateway { get; }

        Task<bool> MakePayment(ChapterPaymentSettings paymentSettings, string currencyCode, double amount, string cardToken,
            string description, string memberName);

        Task<bool> VerifyPayment(ChapterPaymentSettings paymentSettings, string currencyCode, double amount, string cardToken);
    }
}
