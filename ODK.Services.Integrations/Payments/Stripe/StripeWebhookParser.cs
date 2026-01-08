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

    public async Task<PaymentProviderWebhook?> ParseWebhook(string json, string? signature, int version)
    {
        try
        {
            var secret = version == 2
                ? _settings.WebhookSecretV2
                : _settings.WebhookSecretV1;

            var stripeEvent = string.IsNullOrEmpty(secret)
                ? EventUtility.ParseEvent(json)
                : EventUtility.ConstructEvent(json, signature, secret);

            return stripeEvent.Type switch
            {
                EventTypes.CheckoutSessionCompleted => ToCheckoutSessionCompleted(stripeEvent),
                EventTypes.CheckoutSessionExpired => ToCheckoutSessionExpired(stripeEvent),
                EventTypes.InvoicePaymentSucceeded => ToInvoicePaymentSucceeded(stripeEvent),
                EventTypes.PaymentIntentSucceeded => ToPaymentIntentSucceeded(stripeEvent),
                EventTypes.CustomerSubscriptionDeleted => ToSubscriptionDeleted(stripeEvent),
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

    private static PaymentProviderWebhook ToCheckoutSessionExpired(Event stripeEvent)
    {
        var session = (Session)stripeEvent.Data.Object;

        return new PaymentProviderWebhook
        {
            Amount = 0,
            Complete = session.Status == "expired",
            Id = session.Id,
            Metadata = session.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = session.PaymentIntentId,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = null,
            Type = PaymentProviderWebhookType.CheckoutSessionExpired
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

    private static PaymentProviderWebhook ToSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = (Subscription)stripeEvent.Data.Object;

        return new PaymentProviderWebhook
        {
            Amount = 0,
            Complete = subscription.Status == "canceled",
            Id = subscription.Id,
            Metadata = subscription.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = null,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = subscription.Id,
            Type = PaymentProviderWebhookType.SubscriptionCancelled
        };
    }
}