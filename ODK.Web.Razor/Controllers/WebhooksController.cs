using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Tasks;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Common.Extensions;
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
        var env = Request.GetHeader(_appSettings.Brevo.WebhookEnvHeader);
        if (env != _appSettings.Brevo.WebhookEnv)
        {
            return;
        }

        var password = Request.GetHeader(_appSettings.Brevo.WebhookPasswordHeader);
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

        await _loggingService.Info($"Received Stripe webhook: {json}");

        var webhook = await _stripeWebhookParser.ParseWebhook(json, signature, v);
        if (webhook == null)
        {
            return;
        }

        var serviceRequest = ServiceRequest;

        _backgroundTaskService.Enqueue(
            () => _paymentService.ProcessWebhook(serviceRequest, webhook));
    }
}