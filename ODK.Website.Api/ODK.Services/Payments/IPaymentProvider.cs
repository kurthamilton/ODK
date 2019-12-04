using System.Threading.Tasks;

namespace ODK.Services.Payments
{
    public interface IPaymentProvider
    {
        Task<string> MakePayment(string email, string apiSecretKey, string currencyCode, double amount, string token);
    }
}
