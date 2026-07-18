using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Infrastructure.Settings;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class WebhooksController : OdkControllerBase
{
    private readonly AppSettings _appSettings;
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
        AppSettings appSettings,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _appSettings = appSettings;
        _backgroundTaskService = backgroundTaskService;
        _emailService = emailService;
        _loggingService = loggingService;
        _paymentService = paymentService;
        _stripeWebhookParser = stripeWebhookParser;
    }

    [HttpPost("webhooks/brevo")]
    public async Task Brevo()
    {
        var env = GetHeader(_appSettings.Brevo.WebhookEnvHeader);
        if (env != _appSettings.Brevo.WebhookEnv)
        {
            return;
        }

        var password = GetHeader(_appSettings.Brevo.WebhookPasswordHeader);
        if (password != _appSettings.Brevo.WebhookPassword)
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
        if (metadata.Platform == null && Platform != PlatformType.DrunkenKnitwits)
        {
            // Only the DrunkenKnitwits platform will have subscriptions that predate the use of Platform in metadata.
            // Only process these subscriptions on the DrunkenKnitwits platform.
            await _loggingService.Warn(
                $"Received Stripe webhook on platform {Platform} when no Platform was specified in the Stripe metadata. " +
                $"Not processing");
            return;
        }

        if (metadata.Platform != null && metadata.Platform != Platform)
        {
            // Webhooks are set up for both Platforms. All events are sent out to both platforms.
            // Logging a platform mismatch here would create redundant noise in the logs since the
            // event will be handled by the other platform.
            return;
        }

        // Only log our parsed data to avoid logging any PII in the raw JSON
        await _loggingService.Info($"Received Stripe webhook on platform {Platform}: {JsonUtils.Serialize(webhook)}");

        var request = ServiceRequest;
        _backgroundTaskService.Enqueue(
            () => _paymentService.ProcessWebhook(request, webhook),
            BackgroundTaskQueueType.Payments);
    }
}