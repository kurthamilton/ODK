using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentProvider
{
    bool HasExternalGateway { get; }

    Task<string?> CreateOrder(IPaymentSettings paymentSettings, Guid id, 
        string currencyCode, decimal amount, string description);

    Task<ServiceResult> MakePayment(ChapterPaymentSettings paymentSettings, string currencyCode, decimal amount, 
        string cardToken, string description, string memberName);

    Task<ServiceResult> VerifyPayment(ChapterPaymentSettings paymentSettings, string currencyCode, decimal amount, 
        string cardToken);
}
