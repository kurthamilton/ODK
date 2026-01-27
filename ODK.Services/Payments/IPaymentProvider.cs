using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentProvider
{
    Task<ServiceResult> ActivateSubscriptionPlan(string externalId);

    Task<bool> CancelSubscription(string externalId);

    Task<RemoteAccount?> CreateConnectedAccount(CreateRemoteAccountOptions options);

    Task<string?> CreateProduct(string name);

    Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan);

    Task<ServiceResult> DeactivateSubscriptionPlan(string externalId);

    Task<string?> GenerateConnectedAccountSetupUrl(GenerateRemoteAccountSetupUrlOptions options);

    Task<ExternalCheckoutSession?> GetCheckoutSession(string externalId);

    Task<RemoteAccount?> GetConnectedAccount(string externalId);

    Task<string?> GetProductId(string name);

    Task<ExternalSubscription?> GetSubscription(string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId);

    Task<ExternalCheckoutSession> StartCheckout(
        ServiceRequest request,
        string emailAddress,
        ExternalSubscriptionPlan subscriptionPlan,
        string returnPath,
        PaymentMetadataModel metadata);
}