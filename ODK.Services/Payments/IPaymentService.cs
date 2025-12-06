using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<ServiceResult> ActivateSubscriptionPlan(
        IPaymentSettings settings, string? connectedAccountId, string externalId);

    Task<ServiceResult> CancelSubscription(
        IPaymentSettings settings, string? connectedAccountId, string externalId);

    Task<RemoteAccount?> CreatePaymentAccount(IPaymentSettings settings, CreateRemoteAccountOptions options);

    Task<string?> CreateProduct(IPaymentSettings settings, string? connectedAccountId, string name);

    Task<string?> CreateSubscriptionPlan(
        IPaymentSettings settings, string? connectedAccountId, ExternalSubscriptionPlan subscriptionPlan);

    Task<ServiceResult> DeactivateSubscriptionPlan(
        IPaymentSettings settings, string? connectedAccountId, string externalId);

    Task<string?> GeneratePaymentAccountSetupUrl(
        IPaymentSettings settings, GenerateRemoteAccountSetupUrlOptions options);

    Task<ExternalCheckoutSession?> GetCheckoutSession(
        IPaymentSettings settings, string? connectedAccountId, string externalId);

    Task<RemoteAccount?> GetPaymentAccount(IPaymentSettings settings, string externalId);

    Task<string?> GetProductId(IPaymentSettings settings, string? connectedAccountId, string name);

    Task<ExternalSubscription?> GetSubscription(
        IPaymentSettings settings, string? connectedAccountId, string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(
        IPaymentSettings settings, string? connectedAccountId, string externalId);

    Task<ServiceResult> MakePayment(
        IPaymentSettings settings,
        string? connectedAccountId,
        Guid chapterId,
        Currency currency, 
        Member member, 
        decimal amount, 
        string cardToken, 
        string reference);

    Task ProcessWebhook(ServiceRequest request, PaymentProviderWebhook webhook);    

    Task<ExternalCheckoutSession> StartCheckoutSession(
        ServiceRequest request,
        IPaymentSettings settings,
        string? connectedAccountId,
        ExternalSubscriptionPlan subscriptionPlan, 
        string returnPath,
        PaymentMetadataModel metadata);

    Task UpdatePaymentMetadata(
        IPaymentSettings settings, 
        string? connectedAccountId,
        string externalId, 
        PaymentMetadataModel metadata);
}
