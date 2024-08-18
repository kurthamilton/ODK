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

    Task<ExternalSubscription?> GetSubscription(IPaymentSettings settings, string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(IPaymentSettings settings, string externalId);

    Task<ServiceResult> MakePayment(Guid chapterId, Member member, decimal amount, string cardToken, string reference);
}
