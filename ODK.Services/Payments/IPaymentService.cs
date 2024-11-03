using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<ServiceResult> ActivateSubscriptionPlan(IPaymentSettings settings, string externalId);

    Task<ServiceResult> CancelSubscription(IPaymentSettings settings, string externalId);    

    Task<string?> CreateProduct(IPaymentSettings settings, string name);

    Task<string?> CreateSubscriptionPlan(IPaymentSettings settings, ExternalSubscriptionPlan subscriptionPlan);

    Task<ServiceResult> DeactivateSubscriptionPlan(IPaymentSettings settings, string externalId);

    Task<ExternalCheckoutSession?> GetCheckoutSession(IPaymentSettings settings, string externalId);

    Task<string?> GetProductId(IPaymentSettings settings, string name);

    Task<ExternalSubscription?> GetSubscription(IPaymentSettings settings, string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(IPaymentSettings settings, string externalId);

    Task<ServiceResult> MakePayment(ChapterPaymentSettings chapterPaymentSettings, 
        Currency currency, Member member, decimal amount, string cardToken, string reference);

    Task<ExternalCheckoutSession> StartCheckoutSession(
        IPaymentSettings settings, ExternalSubscriptionPlan subscriptionPlan, string returnPath);
}
