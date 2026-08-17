using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Common.Settings;
using ServiceRequestImpl = ODK.Services.ServiceRequest;

namespace ODK.Web.Razor.Controllers;

[ApiController]
[IgnoreAntiforgeryToken] // external POSTs; authenticated by signature/secret, not a token
public class WebhooksController : OdkControllerBase
{
    private readonly WebhooksControllerSettings _settings;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEmailService _emailService;
    private readonly ILoggingService _loggingService;
    private readonly IPaymentService _paymentService;
    private readonly IStripeWebhookParser _stripeWebhookParser;

    public WebhooksController(
        ILoggingService loggingService,
        IPaymentProviderFactory paymentProviderFactory,
        IBackgroundTaskService backgroundTaskService,
        IPaymentService paymentService,
        IStripeWebhookParser stripeWebhookParser,
        IRequestStore requestStore,
        IEmailService emailService,
        WebhooksControllerSettings settings,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _settings = settings;
        _backgroundTaskService = backgroundTaskService;
        _emailService = emailService;
        _loggingService = loggingService;
        _paymentService = paymentService;
        _stripeWebhookParser = stripeWebhookParser;
    }

    [HttpPost("webhooks/brevo")]
    public async Task Brevo()
    {
        var env = GetHeader(_settings.BrevoWebhookEnvHeader);
        if (env != _settings.BrevoWebhookEnv)
        {
            return;
        }

        var password = GetHeader(_settings.BrevoWebhookPasswordHeader);
        if (password != _settings.BrevoWebhookPassword)
        {
            throw new OdkNotAuthenticatedException();
        }

        var json = await ReadBodyText();

        var node = JsonNode.Parse(json);
        if (node == null)
        {
            return;
        }

        var eventName = node?["event"]?.GetValue<string>();
        var externalId = node?["message-id"]?.GetValue<string>();

        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(externalId))
        {
            await _loggingService.Error(
                $"Error processing Brevo webhook: event {eventName} or messageId {externalId}  not found");
            return;
        }

        await _emailService.AddEvent(externalId, eventName);
    }

    /// <summary>
    /// v = 1 for site webhooks.
    /// v = 2 for connected account webhooks.
    /// </summary>
    [HttpPost("webhooks/stripe")]
    public async Task Stripe(int v)
    {
        var signature = Request.Headers["Stripe-Signature"];
        var json = await ReadBodyText();

        var webhook = await _stripeWebhookParser.ParseWebhook(json, signature, v);
        if (webhook == null)
        {
            return;
        }

        var metadata = PaymentMetadataModel.FromDictionary(webhook.Metadata);

        // Only log our parsed data to avoid logging any PII in the raw JSON
        await _loggingService.Info($"Received Stripe webhook: {JsonUtils.Serialize(webhook)}");

        // Webhooks are only set up for one platform to avoid sending redundant webhooks to all.
        // Webhooks without platforms are for older DrunkenKnitwits subscriptions.
        var request = ServiceRequestImpl.Create(ServiceRequest, metadata.Platform ?? PlatformType.DrunkenKnitwits);

        _backgroundTaskService.Enqueue(
            () => _paymentService.ProcessWebhook(request, webhook),
            BackgroundTaskQueueType.Payments);
    }
}