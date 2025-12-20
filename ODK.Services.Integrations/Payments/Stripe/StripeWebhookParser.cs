using ODK.Core.Payments;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using Stripe;
using Stripe.Checkout;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripeWebhookParser : IStripeWebhookParser
{
    private readonly ILoggingService _loggingService;
    private readonly StripeWebhookParserSettings _settings;

    public StripeWebhookParser(
        ILoggingService loggingService,
        StripeWebhookParserSettings settings)
    {
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<PaymentProviderWebhook?> ParseWebhook(string json, string? signature)
    {
        try
        {
            var stripeEvent = string.IsNullOrEmpty(_settings.WebhookSecret) 
                ? EventUtility.ParseEvent(json)
                : EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret);

            return stripeEvent.Type switch
            {
                EventTypes.CheckoutSessionCompleted => ToCheckoutSessionCompleted(stripeEvent),
                EventTypes.InvoicePaymentSucceeded => ToInvoicePaymentSucceeded(stripeEvent),
                EventTypes.PaymentIntentSucceeded => ToPaymentIntentSucceeded(stripeEvent),
                _ => null
            };            
        }
        catch (Exception ex)
        {
            await _loggingService.Error("Error handling Stripe webhook", ex);
            return null;
        }
    }

    private static PaymentProviderWebhook ToCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = (Session)stripeEvent.Data.Object;
        
        return new PaymentProviderWebhook
        {
            Amount = session.AmountTotal > 0 
                ? (decimal)(session.AmountTotal.Value / 100.0) 
                : 0,
            Complete = session.PaymentStatus == "paid",
            Id = session.Id,
            Metadata = session.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = session.PaymentIntentId,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = null,
            Type = PaymentProviderWebhookType.CheckoutSessionCompleted
        };
    }

    private static PaymentProviderWebhook ToInvoicePaymentSucceeded(Event stripeEvent)
    {
        var invoice = (Invoice)stripeEvent.Data.Object;

        return new PaymentProviderWebhook
        {
            Amount = (decimal)(invoice.AmountPaid / 100.0),
            Complete = invoice.Status == "paid",
            Id = invoice.Id,
            Metadata = invoice.Parent.SubscriptionDetails.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = invoice.RawJObject.Value<string>("payment_intent"),
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = invoice.Parent.SubscriptionDetails.SubscriptionId,
            Type = PaymentProviderWebhookType.InvoicePaymentSucceeded
        };
    }

    private static PaymentProviderWebhook ToPaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;

        return new PaymentProviderWebhook
        {
            Amount = (decimal)(paymentIntent.Amount / 100.0),
            Complete = paymentIntent.Status == "succeeded",
            Id = paymentIntent.Id,
            Metadata = paymentIntent.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = paymentIntent.Id,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = null,
            Type = PaymentProviderWebhookType.PaymentSucceeded
        };
    }
}
