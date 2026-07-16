using System.Text.Json;
using ODK.Core.Payments;
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

    public async Task<PaymentProviderWebhook?> ParseWebhook(string json, string? signature, int version)
    {
        var secret = version == 2
            ? _settings.WebhookSecretV2
            : _settings.WebhookSecretV1;

        if (string.IsNullOrWhiteSpace(secret))
        {
            // Configuration error: throw (rather than returning null, which the controller turns into a 200)
            // so the endpoint returns a 5xx and Stripe re-delivers the event once the secret is configured,
            // instead of silently and permanently dropping a genuine event.
            throw new OdkServiceException($"Stripe webhook secret v{version} not set");
        }

        try
        {
            // Constructing the event validates the payload signature against the secret.
            // A direct parse skips signature validation and must only ever be used in tests, so it is gated to
            // debug builds behind an explicit sentinel - production (release) builds are always fail-closed.
            var skipValidation = false;
#if DEBUG
            skipValidation = secret == "IGNORE";
#endif
            var stripeEvent = skipValidation
                ? EventUtility.ParseEvent(json)
                : EventUtility.ConstructEvent(json, signature, secret);

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
            Metadata = subscriptionDetails?.Metadata ?? new Dictionary<string, string>(),
            OriginatedUtc = stripeEvent.Created,
            PaymentId = invoice.RawJsonElement is { } rawJson
                && rawJson.TryGetProperty("payment_intent", out var paymentIntent)
                && paymentIntent.ValueKind == JsonValueKind.String
                ? paymentIntent.GetString()
                : null,
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
            Metadata = subscription.Metadata,
            OriginatedUtc = stripeEvent.Created,
            PaymentId = null,
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = subscription.Id,
            Type = PaymentProviderWebhookType.SubscriptionCancelled
        };
    }
}