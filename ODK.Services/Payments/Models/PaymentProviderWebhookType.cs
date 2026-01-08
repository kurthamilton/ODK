namespace ODK.Services.Payments.Models;

public enum PaymentProviderWebhookType
{
    None,
    CheckoutSessionCompleted,
    CheckoutSessionExpired,
    InvoicePaymentSucceeded,
    PaymentSucceeded,
    SubscriptionCancelled
}