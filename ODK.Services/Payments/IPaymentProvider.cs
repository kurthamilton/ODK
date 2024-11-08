namespace ODK.Services.Payments;

public interface IPaymentProvider
{
    bool HasCustomers { get; }

    bool HasExternalGateway { get; }

    Task<ServiceResult> ActivateSubscriptionPlan(string externalId);

    Task<bool> CancelSubscription(string externalId);    

    Task<string?> CreateCustomer(string emailAddress);

    Task<string?> CreateProduct(string name);

    Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan);

    Task<ServiceResult> DeactivateSubscriptionPlan(string externalId);

    Task<ExternalCheckoutSession?> GetCheckoutSession(string externalId);

    Task<string?> GetProductId(string name);

    Task<ExternalSubscription?> GetSubscription(string externalId);

    Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId);

    Task<ServiceResult> MakePayment(string currencyCode, decimal amount, string cardToken, 
        string description, string memberName);

    Task<string?> SendPayment(string currencyCode, decimal amount, string emailAddress,
        string paymentId, string note);

    Task<ExternalCheckoutSession> StartCheckout(ExternalSubscriptionPlan subscriptionPlan, string returnPath);

    Task<ServiceResult> VerifyPayment(string currencyCode, decimal amount, string cardToken);
}
