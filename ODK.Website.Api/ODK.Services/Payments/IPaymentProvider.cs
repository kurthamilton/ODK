using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Payments
{
    public interface IPaymentProvider
    {
        Task<string> CreatePayment(string email, string apiSecretKey, string currencyCode, ChapterSubscription subscription,
            string successUrl, string cancelUrl);
    }
}
