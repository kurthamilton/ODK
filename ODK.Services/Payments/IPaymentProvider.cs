using ODK.Core.Web;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentProvider
{
    bool HasCustomers { get; }

    bool HasExternalGateway { get; }

    bool SupportsRecurringPayments { get; }

    Task<ServiceResult> ActivateSubscriptionPlan(string externalId);

    Task<bool> CancelSubscription(string externalId);    

    Task<string?> CreateCustomer(string emailAddress);

    Task<string?> CreateProduct(string name);

    Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan);

    Task<ServiceResult> DeactivateSubscriptionPlan(string externalId);

    Task<ExternalCheckoutSession?> GetCheckoutSession(string externalId);

    Task<IReadOnlyCollection<RemotePaymentModel>> GetAllPayments();

    Task<string?> GetProductId(string name);

    Task<ExternalSubscription?> GetSubscription(string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId);

    Task<RemotePaymentResult> MakePayment(string currencyCode, decimal amount, string cardToken, 
        string description, Guid memberId, string memberName);    

    Task<string?> SendPayment(string currencyCode, decimal amount, string emailAddress,
        string paymentId, string note);

    Task<ExternalCheckoutSession> StartCheckout(
        ServiceRequest request, 
        ExternalSubscriptionPlan subscriptionPlan, 
        string returnPath, 
        PaymentMetadataModel metadata);

    Task UpdatePaymentMetadata(string externalId, PaymentMetadataModel metadata);

    Task<ServiceResult> VerifyPayment(string currencyCode, decimal amount, string cardToken);
}
