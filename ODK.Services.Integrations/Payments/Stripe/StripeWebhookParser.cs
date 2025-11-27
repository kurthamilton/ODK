using ODK.Core.Payments;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using System.Text.Json.Nodes;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripeWebhookParser : IStripeWebhookParser
{
    private readonly ILoggingService _loggingService;

    public StripeWebhookParser(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<PaymentProviderWebhook?> ParseWebhook(string json)
    {        
        JsonNode? obj;

        try
        {
            obj = JsonNode.Parse(json);
        }
        catch
        {
            await _loggingService.Error($"Error parsing Stripe webhook: {json}");
            return null;
        }

        if (obj == null)
        {
            return null;
        }

        var objData = obj["data"]?["object"];

        var metadata = new Dictionary<string, string>();
        var objMetadata = objData?["metadata"]?.AsObject();
        if (objMetadata != null)
        {
            foreach (var kvp in objMetadata)
            {
                metadata[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
        }

        return new PaymentProviderWebhook
        {
            Complete = objData?["status"]?.GetValue<string>() == "complete",
            Id = objData?["id"]?.GetValue<string>() ?? string.Empty,
            Metadata = metadata,
            PaymentId = objData?["payment_intent"]?.GetValue<string>(),
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = null,
            Type = obj["type"]?.GetValue<string>() == "checkout.session.completed"
                ? PaymentProviderWebhookType.CheckoutSessionCompleted
                : null
        };
    }
}
