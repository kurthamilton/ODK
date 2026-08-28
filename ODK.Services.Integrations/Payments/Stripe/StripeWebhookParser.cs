using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Exceptions;
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

    public async Task<PaymentProviderWebhook?> ParseWebhook(PlatformType platform, string json, string? signature, int version)
    {
        var webhookSecrets = version == 2
            ? _settings.WebhookSecretsV2
            : _settings.WebhookSecretsV1;

        if (!webhookSecrets.TryGetValue(platform, out var secret) || string.IsNullOrWhiteSpace(secret))
        {
            // Configuration error: throw (rather than returning null, which the controller turns into a 200)
            // so the endpoint returns a 5xx and Stripe re-delivers the event once the secret is configured,
            // instead of silently and permanently dropping a genuine event.
            throw new OdkServiceException($"Stripe webhook secret v{version} not set for platform {platform}");
        }

        try
        {
            // Constructing the event validates the payload signature against the secret.
            var stripeEvent = EventUtility.ConstructEvent(json, signature, secret);

            // Log receipt of every validated event (id and type only - no PII) so that unhandled or
            // dropped event types remain traceable.
            await _loggingService.Info($"Received Stripe webhook '{stripeEvent.Id}' of type '{stripeEvent.Type}'");

            return stripeEvent.Type switch
            {
                EventTypes.CheckoutSessionCompleted => ToCheckoutSessionCompleted(stripeEvent),
                EventTypes.CheckoutSessionExpired => ToCheckoutSessionExpired(stripeEvent),
                EventTypes.InvoicePaymentSucceeded => ToInvoicePaymentSucceeded(stripeEvent),
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
            Id = stripeEvent.Id,
            InvoiceId = null,
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
            Id = stripeEvent.Id,
            InvoiceId = null,
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

        // An invoice not tied to a subscription (or with an unexpected payload shape) has no subscription
        // details. Guard against a NullReferenceException here - a null SubscriptionId is treated downstream
        // as "not a subscription payment" and ignored gracefully rather than crashing (and dropping the event).
        var subscriptionDetails = invoice.Parent?.SubscriptionDetails;

        return new PaymentProviderWebhook
        {
            Amount = (decimal)(invoice.AmountPaid / 100.0),
            Complete = invoice.Status == "paid",
            Id = stripeEvent.Id,
            InvoiceId = invoice.Id,
            Metadata = subscriptionDetails?.Metadata ?? new Dictionary<string, string>(),
            OriginatedUtc = stripeEvent.Created,
            /* An invoice names no payment: the payment intent left the invoice in the same API version
               that introduced the parent read above, so a payload carrying one cannot carry the other.
               InvoiceId is the handle on what was charged - see IPaymentProvider.GetInvoicePaymentId. */
            PaymentId = null,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = subscriptionDetails?.SubscriptionId,
            Type = PaymentProviderWebhookType.InvoicePaymentSucceeded
        };
    }

    private static PaymentProviderWebhook ToSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = (Subscription)stripeEvent.Data.Object;

        return new PaymentProviderWebhook
        {
            Amount = 0,
            Complete = subscription.Status == "canceled",
            Id = stripeEvent.Id,
            InvoiceId = null,
            Metadata = subscription.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = null,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = subscription.Id,
            Type = PaymentProviderWebhookType.SubscriptionCancelled
        };
    }
}