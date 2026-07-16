namespace ODK.Services.Payments.Models;

public enum PaymentProviderWebhookType
{
    None,
    CheckoutSessionCompleted = 1,
    CheckoutSessionExpired = 2,
    InvoicePaymentSucceeded = 3,
    // No longer used - functionality is covered by CheckoutSessionCompleted and InvoicePaymentSucceeded
    // PaymentSucceeded = 4,
    SubscriptionCancelled = 5
}