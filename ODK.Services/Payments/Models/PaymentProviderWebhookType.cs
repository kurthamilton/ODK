namespace ODK.Services.Payments.Models;

public enum PaymentProviderWebhookType
{
    None,
    CheckoutSessionCompleted,
    InvoicePaymentSucceeded,
    PaymentSucceeded
}
