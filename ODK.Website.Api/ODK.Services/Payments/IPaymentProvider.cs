using System.Threading.Tasks;

namespace ODK.Services.Payments
{
    public interface IPaymentProvider
    {
        Task<string> MakePayment(string apiSecretKey, string currencyCode, double amount,
            string cardToken, string description, string memberName);
    }
}
